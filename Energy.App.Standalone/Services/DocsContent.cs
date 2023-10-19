﻿using Ganss.Xss;
using HtmlAgilityPack;
using System.Text;

namespace Energy.App.Standalone.Services;
public class DocsContent
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ISanitization _sanitization;
    public DocsContent(IHttpClientFactory httpClientFactory, ISanitization sanitization)
    {
        _httpClientFactory = httpClientFactory;
        _sanitization = sanitization;
    }


    public async Task<(string content, string docPath)> GetDocSnippet(DocSnippet docSnippet)
    {
        string subpath = SnippetSubpaths[docSnippet];
        string anchor = subpath.Substring(subpath.IndexOf("#") + 1);
        string docPathWithoutAnchor = subpath.Substring(0, subpath.IndexOf("#"));
        var httpClient = _httpClientFactory.CreateClient("DocsSite");
        var htmlSanitizer =
            new HtmlSanitizer(new HtmlSanitizerOptions {
                AllowedTags = new HashSet<string>() { "div", "p", "h1", "h2", "h3", "h4", "h5", "h6", "a", "span", "em", "strong" },
                AllowedSchemes = new HashSet<string>() { "https" },
                
            });
        var responseMessage = await httpClient.GetAsync(docPathWithoutAnchor);

        //string safeDocSnippetDocument = _sanitization.Sanitize(docSnippetDocument);
        var htmlDoc = new HtmlDocument();

        htmlDoc.Load(responseMessage.Content.ReadAsStream());
        var snippetNode = htmlDoc.DocumentNode.SelectSingleNode($"//main//*[@id='{anchor}']");
        //var parentSnippetNode = snippetNode.ParentNode;
        //var snippetNodes = parentSnippetNode.SelectNodes(".//following-sibling::p[preceding::h3]");
        var current = snippetNode;
        StringBuilder content = new StringBuilder();
        //content.Append(htmlSanitizer.Sanitize(snippetNode.OuterHtml)); // Add the first header to your content.

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
                string innerText = htmlSanitizer.Sanitize(current.InnerText);
                if(String.IsNullOrWhiteSpace(innerText))
                {
                    continue;
                }
                content.Append(innerText);
                content.Append("<br/><br/>");
            }
        }
        string sourceUrl = responseMessage.RequestMessage.RequestUri.ToString();

        string finalContent = content.ToString();
        return (content: finalContent, docPath: sourceUrl);
    }

    public static Dictionary<DocSnippet, string> SnippetSubpaths = new Dictionary<DocSnippet, string>() {
        {DocSnippet.HouseholdIHD, "setup/household.html#smart-meter-ihd-id"},
        {DocSnippet.HouseholdMoveInDate, "setup/household.html#move-in-date"},
        {DocSnippet.HouseholdPostalArea, "setup/household.html#postal-area"},
        {DocSnippet.HouseholdPrimaryHeatingSource, "setup/household.html#primary-heating-source"},
        {DocSnippet.GasMeterMPRN, "setup/gas-meter.html#gas-meter-mprn"},
        {DocSnippet.ElectricityMeterMPAN, "setup/electricity-meter.html#electricity-meter-mpan"},
        };
}




public enum DocSnippet
{
    HouseholdIHD,
    HouseholdMoveInDate,
    HouseholdPostalArea,
    HouseholdPrimaryHeatingSource,
    GasMeterMPRN,
    ElectricityMeterMPAN,
    AuthorizationPresetFields,
    AuthorizationN3rgyPage,
    AuthorizationCheckButton,
}