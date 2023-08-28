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

    })
    .option('licenseUrlToLicenseTypeOverrideFile', {
        type: 'string',
        demandOption: false,
        describe: 'The path to the license url to license type override file, which overrides the license type for a given license url. Defaults to the script directory/NugetLicenseUrlToLicenseTypeOverride.json',
    }).option('licensePlainTextFolder', {
        type: 'string',
        demandOption: false,
        describe: 'The path to the folder containing the plain text spdx license files. Defaults to the script directory/SpdxLicensePlainTextFiles',
    }).option('generatedHtmlDocumentPath', {
        type: 'string',
        demandOption: false,
        describe: 'The path to the generated html document. Defaults to the script directory/NugetCreditsPartial.html',
    }).option('tempLicenseOutputFolder', {
        type: 'string',
        demandOption: false,
        describe: 'The path to the temporary license output folder where the license generation will take place. Defaults to the script directory/NugetLicenseTempOutput',
    }).option('nugetCreditsTemplateFile', {
        type: 'string',
        demandOption: false,
        describe: 'The path to the nuget credits template file. Defaults to the script directory/NugetCreditsPartialTemplate.hbs',
    }).argv;


const scriptDirectory = path.dirname(process.argv[1]);
console.log(`Script directory: ${scriptDirectory}`);

const licenseInfoOverrideFile = argv.licenseInfoOverrideFile ? argv.licenseInfoOverrideFile : path.join(scriptDirectory, './LicenseInfoOverride.json');
const licenseUrlToLicenseTypeOverrideFile = argv.licenseUrlToLicenseTypeOverrideFile ? argv.licenseUrlToLicenseTypeOverrideFile : path.join(scriptDirectory, './LicenseUrlToLicenseTypeOverride.json');
const licensePlainTextFolder = argv.licensePlainTextFolder ? argv.licensePlainTextFolder : path.join(scriptDirectory, './SpdxLicensePlainTextFiles');
const generatedHtmlDocumentPath = argv.generatedHtmlDocumentPath ? argv.generatedHtmlDocumentPath : path.join(scriptDirectory, './NugetCreditsPartial.html');
const nugetCreditsTemplateFile = argv.nugetCreditsTemplateFile ? argv.nugetCreditsTemplateFile : path.join(scriptDirectory, './NugetCreditsPartialTemplate.hbs');
const tempLicenseOutputFolder = argv.tempLicenseOutputFolder ? argv.tempLicenseOutputFolder : path.join(scriptDirectory, './NugetLicenseTempOutput');

const parentProjectPath = path.resolve(argv.parentProjectPath);

try {
    console.log('Clearing NuGet caches');
    execSync('dotnet nuget locals all --clear');

    console.log('Restoring project packages');
    execSync(`dotnet restore ${parentProjectPath}`);

    if (fs.existsSync(tempLicenseOutputFolder)) {
        fs.rmSync(tempLicenseOutputFolder, { recursive: true });
    }
    fs.mkdirSync(tempLicenseOutputFolder);

    const tempPackageLicenseJsonFileOutput = path.join(tempLicenseOutputFolder, 'Licenses.json');

    const licenseInfoOverrideParsed = JSON.parse(fs.readFileSync(licenseInfoOverrideFile), 'utf8');
    const tempLicenseOverridePackageNamesFile = path.join(tempLicenseOutputFolder, 'LicenseOverridePackageNames.json');

    fs.writeFileSync(tempLicenseOverridePackageNamesFile, JSON.stringify(licenseInfoOverrideParsed.map(c => c.PackageName)), 'utf8');

    execSync(`dotnet-project-licenses -i ${parentProjectPath} -u -t -o -j  --use-project-assets-json  --outfile ${tempPackageLicenseJsonFileOutput} --packages-filter ${tempLicenseOverridePackageNamesFile} --manual-package-information ${licenseInfoOverrideFile} --licenseurl-to-license-mappings ${licenseUrlToLicenseTypeOverrideFile}`, { cwd: tempLicenseOutputFolder });

    const parsedPackageInfos = JSON.parse(fs.readFileSync(tempPackageLicenseJsonFileOutput, 'utf8'));

    const encodedPackageInfosWithLicenseText = [];
    for (const parsedPackageInfo of parsedPackageInfos) {

        const licenseTextFilePath = path.join(licensePlainTextFolder, `${parsedPackageInfo.LicenseType}.txt`);

        if (!fs.existsSync(licenseTextFilePath)) {
            throw new Error(`License text file not found: ${licenseTextFilePath}`);
        }
        const licensePlainText = fs.readFileSync(licenseTextFilePath, 'utf8');
        const licensePlainTextEncoded = he.encode(licensePlainText, { strict: true });

        const AuthorsJoined = parsedPackageInfo.Authors && parsedPackageInfo.Authors.length > 0 ? "Authors: " + parsedPackageInfo.Authors.join(', ') : '';
        const AuthorsEncoded = he.encode(AuthorsJoined, { strict: true });

        const LicenseTypeEncoded = he.encode(parsedPackageInfo.LicenseType, { strict: true });

        const CopyrightEncoded = he.encode(parsedPackageInfo.Copyright, { strict: true });

        const PackageNameEncoded = he.encode(parsedPackageInfo.PackageName, { strict: true });

        const encodedPackageInfo = { PackageName: PackageNameEncoded, Copyright: CopyrightEncoded, LicenseType: LicenseTypeEncoded, Authors: AuthorsEncoded, LicenseTextEncodedToInsert: licensePlainTextEncoded };

        encodedPackageInfosWithLicenseText.push(encodedPackageInfo);
    }

    const templateText = fs.readFileSync(nugetCreditsTemplateFile, 'utf8');

    const template = Handlebars.compile(templateText, { preventIndent: true, strict: true, noEscape: true });
    const htmlRendered = template({ packageInfos: encodedPackageInfosWithLicenseText });

    const htmlSanitized = DOMPurify.sanitize(htmlRendered, { USE_PROFILES: { html: true } });

    console.log('DOMPurify removed %d elements.', DOMPurify.removed.length)

    if (DOMPurify.removed.length > 0) {
        console.log('Elements removed:  %s', JSON.stringify(DOMPurify.removed));
    }

    fs.writeFileSync(generatedHtmlDocumentPath, htmlSanitized, 'utf8');


} catch (error) {
    console.error(error);
    console.error(error.message);
    console.error(error.stack);
    process.exitCode = 1;

} finally {

    console.log('Clearing temporary license output folder');
    if (fs.existsSync(tempLicenseOutputFolder)) {
        fs.rmSync(tempLicenseOutputFolder, { recursive: true });
    }

    console.log('Completed')
}