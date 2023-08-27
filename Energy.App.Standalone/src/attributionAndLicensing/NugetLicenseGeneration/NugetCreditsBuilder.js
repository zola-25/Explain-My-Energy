import { execSync } from 'child_process';
import fs from 'fs';
import path from 'path';
import yargs from 'yargs';
import { hideBin } from 'yargs/helpers';
import he from 'he';
import DOMPurify from "isomorphic-dompurify";
import Handlebars from "handlebars";

const argv = yargs(hideBin(process.argv))
    .option('parentProjectPath', {
        type: 'string',
        demandOption: true,
        describe: 'The path to the .csproj to generate license the license html for',
    })
    .option('licenseInfoOverrideFile', {
        type: 'string',
        demandOption: false,
        describe: 'The path to the license info override file, which overrides all license info for a package. Defaults to the script directory/LicenseInfoOverride.json',
        default: './NugetLicenseInfoOverride.json',
    })
    .option('licenseUrlToLicenseTypeOverrideFile', {
        type: 'string',
        demandOption: false,
        describe: 'The path to the license url to license type override file, which overrides the license type for a given license url. Defaults to the script directory/NugetLicenseUrlToLicenseTypeOverride.json',
        default: './NugetLicenseUrlToLicenseTypeOverride.json',
    }).option('licensePlainTextFolder', {
        type: 'string',
        demandOption: false,
        describe: 'The path to the folder containing the plain text spdx license files. Defaults to the script directory/SpdxLicensePlainTextFiles',
        default: './SpdxLicensePlainTextFiles',
    }).option('generatedHtmlDocumentPath', {
        type: 'string',
        demandOption: false,
        describe: 'The path to the generated html document. Defaults to the script directory/NugetCredits.html',
    }).option('tempLicenseOutputFolder', {
        type: 'string',
        demandOption: false,
        describe: 'The path to the temporary license output folder where the license generation will take place. Defaults to the script directory/NugetLicenseTempOutput',
        default: './NugetLicenseTempOutput',
    }).option('nugetCreditsTemplateFile', {
        type: 'string',
        demandOption: false,
        describe: 'The path to the nuget credits template file. Defaults to the script directory/NugetCreditsPartialTemplate.hbs',
        default: './NugetCreditsPartialTemplate.hbs'
    }).argv;

const scriptDirectory = path.dirname(process.argv[1]);
const tempLicenseOutputFolder = path.join(scriptDirectory, argv.tempLicenseOutputFolder);

console.log(`Script directory: ${scriptDirectory}`);

try {
    console.log('Clearing NuGet caches');
    execSync('dotnet nuget locals all --clear');

    console.log('Restoring project packages');
    execSync(`dotnet restore ${argv.parentProjectPath}`);

    if (fs.existsSync(tempLicenseOutputFolder)) {
        fs.rmSync(tempLicenseOutputFolder, { recursive: true });
    }
    fs.mkdirSync(tempLicenseOutputFolder);

    const tempPackageLicenseJsonFileOutput = path.join(tempLicenseOutputFolder, 'Licenses.json');

    const licenseInfoOverride = JSON.parse(fs.readFileSync(argv.licenseInfoOverrideFile));
    const tempLicenseOverridePackageNamesFile = path.join(tempLicenseOutputFolder, 'LicenseOverridePackageNames.json');

    fs.writeFileSync(tempLicenseOverridePackageNamesFile, JSON.stringify(licenseInfoOverride.PackageName));

    execSync(`dotnet-project-licenses -i ${argv.parentProjectPath} -u -t -o -j  --use-project-assets-json  --outfile ${tempPackageLicenseJsonFileOutput} --packages-filter ${tempLicenseOverridePackageNamesFile} --manual-package-information ${argv.licenseInfoOverrideFile} --licenseurl-to-license-mappings ${argv.licenseUrlToLicenseTypeOverrideFile}`, { cwd: tempLicenseOutputFolder });

    const packageInfos = JSON.parse(fs.readFileSync(tempPackageLicenseJsonFileOutput));

    const packageInfosWithLicenseText = [];
    for (const packageInfo of packageInfos) {

        const licenseTextFile = path.join(argv.licensePlainTextFolder, `${packageInfo.LicenseType}.txt`);

        if (!fs.existsSync(licenseTextFile)) {
            throw new Error(`License text file not found: ${licenseTextFile}`);
        }
        const licensePlainText = fs.readFileSync(licenseTextFile, 'utf8');
        const licensePlainTextEncoded = he.encode(licensePlainText);

        delete packageInfo.LicenseUrl;

        const packageInfoWithLicenseText = { ...packageInfo, LicenseTextEncodedToInsert: licensePlainTextEncoded };

        packageInfosWithLicenseText.push(packageInfoWithLicenseText);
    }

    const templateText = fs.readFileSync(argv.nugetCreditsTemplateFile, 'utf8');

    const template = Handlebars.compile(templateText, { preventIndent: true, strict: true, noEscape: true });
    const htmlPartial = template({ packageInfos: packageInfosWithLicenseText });

    const html = DOMPurify.sanitize(htmlPartial, { USE_PROFILES: { html: true } });

    console.log(DOMPurify.removed.map((x) => JSON.stringify(x)).join(", ") + " tags were removed from the html");

    fs.writeFileSync(argv.generatedHtmlDocumentPath, html);


} catch (error) {
    console.error(error.message);
    console.error(error.stack);

    process.exitCode = 1;

} finally {

    if (fs.existsSync(tempLicenseOutputFolder)) {
        fs.rmSync(tempLicenseOutputFolder, { recursive: true });
    }

}