using AngleSharp.Html.Dom;
using Energy.Shared;
using Ganss.Xss;
using HtmlAgilityPack;
using System.Text;

namespace Energy.App.Standalone.Services.DocSnippets;
public class DocSnippetsLoader
{
    private readonly ILogger<DocSnippetsLoader> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    private static readonly Dictionary<DocSnippetType, SnippetLoadResult> SavedSnippets = new();

    private static readonly HtmlSanitizerOptions DefaultHtmlSanitizerOptions =
        new()
        {
            AllowedTags = new HashSet<string>() { "div", "p", "h1", "h2", "h3", "h4", "h5", "h6", "a", "span", "em", "strong", "ul", "ol", "li" },
            AllowedAttributes = new HashSet<string>() { "href", "alt" },
            UriAttributes = new HashSet<string>() { "href" },
            AllowedSchemes = new HashSet<string>() { "https" },
        };

    private static readonly Dictionary<DocSnippetType, SnippetCreationDetails> SnippetCreationDetailsLookup = new() {
        {DocSnippetType.AppSetupStatus, new SnippetCreationDetails { Subpath = "setup/app-setup-status.html", Anchor = "#app-setup-status", HtmlSanitizerOptions = DefaultHtmlSanitizerOptions } },
        {DocSnippetType.SetupWizard, new SnippetCreationDetails { Subpath = "setup/setup-wizard.html", Anchor = "#setup-wizard", HtmlSanitizerOptions = DefaultHtmlSanitizerOptions } },
        {DocSnippetType.HouseholdIHD, new SnippetCreationDetails { Subpath = "setup/household.html", Anchor = "#smart-meter-ihd-mac-id", HtmlSanitizerOptions = DefaultHtmlSanitizerOptions } },
        {DocSnippetType.HouseholdMoveInDate, new SnippetCreationDetails { Subpath = "setup/household.html", Anchor = "#move-in-date" , HtmlSanitizerOptions = DefaultHtmlSanitizerOptions} },
        {DocSnippetType.HouseholdPostalArea, new SnippetCreationDetails { Subpath = "setup/household.html", Anchor = "#postal-area" , HtmlSanitizerOptions = DefaultHtmlSanitizerOptions} },
        {DocSnippetType.HouseholdPrimaryHeatingSource, new SnippetCreationDetails { Subpath = "setup/household.html", Anchor = "#primary-heating-source" , HtmlSanitizerOptions = DefaultHtmlSanitizerOptions} },
        {DocSnippetType.GasMeterMPRN, new SnippetCreationDetails { Subpath = "setup/gas-meter.html", Anchor = "#gas-meter-mprn" , HtmlSanitizerOptions = DefaultHtmlSanitizerOptions} },
        {DocSnippetType.ElectricityMeterMPAN, new SnippetCreationDetails { Subpath = "setup/electricity-meter.html", Anchor = "#electricity-meter-mpan" , HtmlSanitizerOptions = DefaultHtmlSanitizerOptions} },
        {DocSnippetType.AuthorizationN3rgyPage, new SnippetCreationDetails { Subpath = "setup/authorization.html", Anchor = "#authorization-process" , HtmlSanitizerOptions = DefaultHtmlSanitizerOptions} },
        {DocSnippetType.WeatherDataStatus, new SnippetCreationDetails { Subpath = "setup/local-weather.html", Anchor = "#weather-data-status" , HtmlSanitizerOptions = DefaultHtmlSanitizerOptions} },
        {DocSnippetType.WeatherDataTable, new SnippetCreationDetails { Subpath = "setup/local-weather.html", Anchor = "#weather-data-table" , HtmlSanitizerOptions = DefaultHtmlSanitizerOptions} },
        {DocSnippetType.ManualActionsUpdateMeterData, new SnippetCreationDetails { Subpath = "setup/manual-actions.html", Anchor = "#update-or-reload-meter-readings" , HtmlSanitizerOptions = DefaultHtmlSanitizerOptions} },
        {DocSnippetType.ManualActionsRemoveMeter, new SnippetCreationDetails { Subpath = "setup/manual-actions.html", Anchor = "#removing-meters" , HtmlSanitizerOptions = DefaultHtmlSanitizerOptions} },
        {DocSnippetType.ManualActionsWipeAllData, new SnippetCreationDetails { Subpath = "setup/manual-actions.html", Anchor = "#wipe-all-data" , HtmlSanitizerOptions = DefaultHtmlSanitizerOptions} },
        {DocSnippetType.EditingTariffs, new SnippetCreationDetails { Subpath = "setup/tariffs.html", Anchor = "#editing-tariffs" , HtmlSanitizerOptions = DefaultHtmlSanitizerOptions} },
        {DocSnippetType.AnalysisOptionsStatus, new SnippetCreationDetails { Subpath = "analysis/analysis-options.html", Anchor = "#analysis-options-status" , HtmlSanitizerOptions = DefaultHtmlSanitizerOptions} },
        {DocSnippetType.AnalysisOptionsChartKWhVsCost, new SnippetCreationDetails { Subpath = "analysis/analysis-options.html", Anchor = "#chart-kwh-vs-cost" , HtmlSanitizerOptions = DefaultHtmlSanitizerOptions} },
        {DocSnippetType.AnalysisOptionsAnalysisTerm, new SnippetCreationDetails { Subpath = "analysis/analysis-options.html", Anchor = "#analysis-term" , HtmlSanitizerOptions = DefaultHtmlSanitizerOptions} },
        {DocSnippetType.AnalysisOptionsTemperatureVsHistorical, new SnippetCreationDetails { Subpath = "analysis/analysis-options.html", Anchor = "#temperature-forecast-vs-historical" , HtmlSanitizerOptions = DefaultHtmlSanitizerOptions} },
        {DocSnippetType.AnalysisOptionsAdjustingForecast, new SnippetCreationDetails { Subpath = "analysis/analysis-options.html", Anchor = "#adjusting-temperature-forecast" , HtmlSanitizerOptions = DefaultHtmlSanitizerOptions} },
        {DocSnippetType.ChartTemperatureDependent, new SnippetCreationDetails { Subpath = "analysis/understanding-chart-data.html", Anchor = "#temperature-dependent-energy-chart" , HtmlSanitizerOptions = DefaultHtmlSanitizerOptions} },
        {DocSnippetType.ChartTemperatureIndependent, new SnippetCreationDetails { Subpath = "analysis/understanding-chart-data.html", Anchor = "#temperature-independent-energy-chart" , HtmlSanitizerOptions = DefaultHtmlSanitizerOptions  } },
    };

    private record SnippetCreationDetails
    {
        public string Subpath { get; init; }
        public string Anchor { get; init; }

        public HtmlSanitizerOptions HtmlSanitizerOptions { get; init; }
    }

    private enum SnippetLoadSource
    {
        AppInitialization,
        AppRuntime
    }

    public DocSnippetsLoader(ILogger<DocSnippetsLoader> logger, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    public async ValueTask PreloadAllSnippets()
    {
        try
        {
            var snippetsBySubpathGroups = SnippetCreationDetailsLookup.GroupBy(c => c.Value.Subpath);

            var httpClient = _httpClientFactory.CreateClient("DocsSite");

            foreach (var snippetsBySubpathGroup in snippetsBySubpathGroups)
            {
                try
                {
                    string subpath = snippetsBySubpathGroup.Key;
                    var responseMessage = await httpClient.GetAsync(subpath);
                    string sourceUrl = responseMessage.RequestMessage.RequestUri.ToString();

                    var htmlDoc = new HtmlDocument();

                    htmlDoc.Load(responseMessage.Content.ReadAsStream());
                    var docSnippetAnchorPairs = snippetsBySubpathGroup.Select(c => (DocSnippet: c.Key, SnippetCreationDetails: c.Value));

                    SaveSnippetsFromDocument(SnippetLoadSource.AppInitialization, sourceUrl, htmlDoc, docSnippetAnchorPairs);
                }
                catch (Exception ex)
                {
                    _logger.LogError(LogEventIds.AppInitializationErrorLoadingSnippetDocPage,
                        ex,
                        message: "Error loading snippets from {SubPath}",
                        snippetsBySubpathGroup.Key);
                }
            }

            if (SavedSnippets.Count == 0)
            {
                throw new AllSnippetsFailedInitializationException();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(LogEventIds.AppInitializationErrorLoadingAllSnippets,
                ex,
                message: "Unable to load any snippets from documentation");
        }
    }


    public async Task<SnippetLoadResult> GetIndividualDocSnippet(DocSnippetType docSnippet)
    {
        try
        {
            if (SavedSnippets.ContainsKey(docSnippet))
            {
                return SavedSnippets[docSnippet];
            }

            var snippetCreationDetails = SnippetCreationDetailsLookup[docSnippet];
            var httpClient = _httpClientFactory.CreateClient("DocsSite");

            var responseMessage = await httpClient.GetAsync(snippetCreationDetails.Subpath);
            string sourceUrl = responseMessage.RequestMessage.RequestUri.ToString();

            var htmlDoc = new HtmlDocument();

            htmlDoc.Load(responseMessage.Content.ReadAsStream());

            SaveSnippetsFromDocument(SnippetLoadSource.AppRuntime, sourceUrl, htmlDoc, new[] { (docSnippet, snippetCreationDetails) });
            return SavedSnippets[docSnippet];
        }
        catch (Exception ex)
        {
            _logger.LogError(LogEventIds.AppRuntimeSnippetFailedToLoad,
                ex,
                message: "Unable to load snippet for {DocSnippet} from documentation",
                docSnippet.ToString());

            return new SnippetLoadResult()
            {
                Error = true,
                Content = "Unable to load snippet from documentation",
                DocPath = "/"
            };
        }
    }


    private void SaveSnippetsFromDocument(
                    SnippetLoadSource snippetLoadSource,
                    string sourceUrl,
                    HtmlDocument htmlDoc,
                    IEnumerable<(DocSnippetType DocSnippet, SnippetCreationDetails SnippetCreationDetails)> docSnippetsCreationDetails)
    {
        foreach (var docSnippetCreationDetails in docSnippetsCreationDetails)
        {
            try
            {
                var docSnippet = docSnippetCreationDetails.DocSnippet;

                var snippetCreationDetails = docSnippetCreationDetails.SnippetCreationDetails;
                string anchorId = snippetCreationDetails.Anchor.TrimStart('#');
                var snippetNode = htmlDoc.DocumentNode.SelectSingleNode($"//main//*[@id='{anchorId}']");

                var htmlSanitizer = new HtmlSanitizer(snippetCreationDetails.HtmlSanitizerOptions);
                htmlSanitizer.PostProcessNode += (sender, e) => {
                    if(e.Node is IHtmlAnchorElement a)
                    {
                        a.SetAttribute("target", "_blank");
                        e.Node.TextContent += " (new tab)";

                        a.RelationList.Add("noopener");
                        a.RelationList.Add("noreferrer");
                    }
                };

                var newSnippet = GetSnippetFromAnchorNode(snippetNode, sourceUrl, htmlSanitizer);
                SaveSnippet(docSnippet, newSnippet);
            }
            catch (Exception ex)
            {
                if (snippetLoadSource == SnippetLoadSource.AppRuntime)
                {
                    throw;
                }

                _logger.LogError(
                    LogEventIds.AppInitializationErrorLoadingIndividualSnippet,
                    ex,
                    message: "Unable to load individual {DocSnippet} from documentation from {SubPath} during App Initialization",
                    docSnippetCreationDetails.DocSnippet.ToString(),
                    docSnippetCreationDetails.SnippetCreationDetails.Subpath);
            }
        }
    }

    private static void SaveSnippet(DocSnippetType docSnippet, SnippetLoadResult newSnippet)
    {
        if (!SavedSnippets.ContainsKey(docSnippet))
        {
            SavedSnippets.Add(docSnippet, newSnippet);
        }
        else
        {
            SavedSnippets[docSnippet] = newSnippet;
        }
    }

    private static SnippetLoadResult GetSnippetFromAnchorNode(HtmlNode snippetNode, string sourceUrl, HtmlSanitizer htmlSanitizer)
    {

        var current = snippetNode;
        var sbSnippetContent = new StringBuilder();

        while (current != null)
        {
            current = current.NextSibling;

            // Stop if you reach another header element (h1, h2, ..., h6).
            if (current != null && current.Name.StartsWith("h"))
            {
                break;
            }

            if (current != null && current.Name == "img")
            {
                continue;
            }

            if (current != null)
            {
                string outerHtml = htmlSanitizer.Sanitize(current.OuterHtml, baseUrl: sourceUrl);
                if (String.IsNullOrWhiteSpace(outerHtml))
                {
                    continue;
                }
                _ = sbSnippetContent
                    .Append(outerHtml);
            }
        }

        string finalContent = sbSnippetContent.ToString();

        var newSnippet = new SnippetLoadResult()
        {
            Error = false,
            Content = finalContent,
            DocPath = sourceUrl
        };
        return newSnippet;
    }
}
