#!/usr/bin/env node

import { execSync, spawnSync } from 'child_process';
import fs from 'fs';
import path from 'path';
import yargs from 'yargs';
import { hideBin } from 'yargs/helpers';
import he from 'he';
import DOMPurify from "isomorphic-dompurify";
import Handlebars from "handlebars";

const argv = yargs(hideBin(process.argv))
    .option('projectCsprojPath', {
        type: 'string',
        demandOption: true,
        describe: 'The path to the .csproj to generate license the license html for',
    })
    .option('licenseInfoOverrideFile', {
        type: 'string',
        demandOption: false,
        describe: 'The path to the license info override file, which overrides all license info for a package. Defaults to the current script directory/LicenseInfoOverride.json',

    })
    .option('licenseUrlToLicenseTypeOverrideFile', {
        type: 'string',
        demandOption: false,
        describe: 'The path to the license url to license type override file, which overrides the license type for a given license url. Defaults to the current script directory/NugetLicenseUrlToLicenseTypeOverride.json',
    }).option('licensePlainTextFolder', {
        type: 'string',
        demandOption: false,
        describe: 'The path to the folder containing the plain text spdx license files. Defaults to the current script directory/SpdxLicensePlainTextFiles',
    }).option('htmlFragmentToGenerateFilePath', {
        type: 'string',
        demandOption: false,
        describe: 'The path to the generated html document. Defaults to the current generatedPartialsFolder/NugetCreditsPartial.html',
    }).
    option('generatedPartialsFolder',{
        type: 'string',
        demandOption: false,
        describe: 'The path to the generated partials folder. Defaults to project root/src/attributionAndLicensing/generatedPartials',
    }).option('tempLicenseOutputFolder', {
        type: 'string',
        demandOption: false,
        describe: 'The path to the temporary license output folder where the license generation will take place. Defaults to the current script directory/NugetLicenseTempOutput',
    }).option('nugetCreditsTemplateFile', {
        type: 'string',
        demandOption: false,
        describe: 'The path to the nuget credits template file. Defaults to the current script directory/NugetCreditsPartialTemplate.hbs',
    }).argv;


const scriptDirectory = path.dirname(process.argv[1]);
const scriptName = path.basename(process.argv[1]);

console.log(`Running ${scriptName} in Script directory: ${scriptDirectory}`);

const tempLicenseOutputFolder = argv.tempLicenseOutputFolder ? argv.tempLicenseOutputFolder : path.join(scriptDirectory, './NugetLicenseTempOutput');

const projectCsprojPath = path.resolve(argv.projectCsprojPath);
const generatedPartialsFolder = argv.generatedPartialsFolder ? argv.generatedPartialsFolder : path.resolve(projectCsprojPath, 'src/attributionAndLicensing/generatedPartials');
const htmlFragmentToGenerateFilePath = argv.htmlFragmentToGenerateFilePath ? argv.htmlFragmentToGenerateFilePath : path.join(generatedPartialsFolder, 'NugetCreditsPartial.html');

try {

    const licenseInfoOverrideFile = argv.licenseInfoOverrideFile ? argv.licenseInfoOverrideFile : path.join(scriptDirectory, './LicenseInfoOverride.json');
    const licenseUrlToLicenseTypeOverrideFile = argv.licenseUrlToLicenseTypeOverrideFile ? argv.licenseUrlToLicenseTypeOverrideFile : path.join(scriptDirectory, './LicenseUrlToLicenseTypeOverride.json');
    const licensePlainTextFolder = argv.licensePlainTextFolder ? argv.licensePlainTextFolder : path.join(scriptDirectory, './SpdxLicensePlainTextFiles');
    const nugetCreditsTemplateFile = argv.nugetCreditsTemplateFile ? argv.nugetCreditsTemplateFile : path.join(scriptDirectory, './NugetCreditsPartialTemplate.hbs');


    console.log('Clearing NuGet caches');
    spawnSync('dotnet nuget locals all --clear', { shell: false});

    console.log('Restoring project packages');
    const dotnetRestoreArgs = ['restore', projectCsprojPath];
    spawnSync("dotnet", dotnetRestoreArgs, { shell: false, stdio: 'inherit' });
    spawnSync("dotnet", [ "tool", "restore"], { shell: false , stdio: 'inherit'});


    if (fs.existsSync(tempLicenseOutputFolder)) {
        fs.rmSync(tempLicenseOutputFolder, { recursive: true });
    }
    fs.mkdirSync(tempLicenseOutputFolder);

    const tempPackageLicenseJsonFileOutput = path.join(tempLicenseOutputFolder, 'Licenses.json');

    const licenseInfoOverrideParsed = JSON.parse(fs.readFileSync(licenseInfoOverrideFile), 'utf8');
    const tempLicenseOverridePackageNamesFile = path.join(tempLicenseOutputFolder, 'LicenseOverridePackageNames.json');

    fs.writeFileSync(tempLicenseOverridePackageNamesFile, JSON.stringify(licenseInfoOverrideParsed.map(c => c.PackageName)), 'utf8');

    const dotnetProjectLicensesArgs = ['tool', 'run', 'nuget-license','-i', projectCsprojPath, '-t', 
        '-o', 'Json', 
        '--file-output', tempPackageLicenseJsonFileOutput, 
        //'--override-package-information', licenseInfoOverrideFile, 
        '--licenseurl-to-license-mappings', licenseUrlToLicenseTypeOverrideFile]
    
    spawnSync("dotnet", dotnetProjectLicensesArgs, { shell: false, stdio: 'inherit', cwd: tempLicenseOutputFolder });

    const parsedPackageInfos = JSON.parse(fs.readFileSync(tempPackageLicenseJsonFileOutput, 'utf8'));

    const encodedPackageInfosWithLicenseText = [];
    for (const parsedPackageInfo of parsedPackageInfos) {

        const licenseTextFilePath = path.join(licensePlainTextFolder, `${parsedPackageInfo.License}.txt`);

        if (!fs.existsSync(licenseTextFilePath)) {
            console.warn(`License text file not found: ${licenseTextFilePath}`);
            continue; // Skip this package if the license text file does not exist
        }
        const licensePlainText = fs.readFileSync(licenseTextFilePath, 'utf8');
        const licensePlainTextEncoded = he.encode(licensePlainText, { strict: true });

        const AuthorsJoined = parsedPackageInfo.Authors && parsedPackageInfo.Authors.length > 0 ? "Authors: " + parsedPackageInfo.Authors : '';
        const AuthorsEncoded = he.encode(AuthorsJoined, { strict: true });

        const LicenseTypeEncoded = he.encode(parsedPackageInfo.License, { strict: true });

        const CopyrightEncoded = he.encode(parsedPackageInfo.Copyright, { strict: true });

        const PackageNameEncoded = he.encode(parsedPackageInfo.PackageId, { strict: true });

        const encodedPackageInfo = { PackageName: PackageNameEncoded, Copyright: CopyrightEncoded, LicenseType: LicenseTypeEncoded, Authors: AuthorsEncoded, LicenseTextEncodedToInsert: licensePlainTextEncoded };

        encodedPackageInfosWithLicenseText.push(encodedPackageInfo);
    }

    const templateText = fs.readFileSync(nugetCreditsTemplateFile, 'utf8');

    const template = Handlebars.compile(templateText, { preventIndent: true, strict: true, noEscape: true });
    const htmlRendered = template({ packageInfos: encodedPackageInfosWithLicenseText });

    DOMPurify.addHook('afterSanitizeAttributes', function (node) {
        // Add rel="noopener noreferrer" to all links that open in a new tab
        if (node.tagName === 'A' && node.getAttribute('target') === '_blank') {
            node.setAttribute('rel', 'noopener noreferrer nofollow');
        }
        // Add rel="nofollow" only to links that do not open in a new tab
        else if (node.tagName === 'A' && node.hasAttribute('href')) {
            node.setAttribute('rel', 'nofollow');
        }

    });

    const htmlSanitized = DOMPurify.sanitize(htmlRendered, { USE_PROFILES: { html: true } });

    console.log('DOMPurify removed %d elements.', DOMPurify.removed.length)

    if (DOMPurify.removed.length > 0) {
        console.log('Elements removed:  %s', JSON.stringify(DOMPurify.removed));
    }

    fs.writeFileSync(htmlFragmentToGenerateFilePath, htmlSanitized, 'utf8');


} catch (error) {
    process.exitCode = 1;

    console.error(error);
    console.error(error.message);
    console.error(error.stack);

} finally {

    console.log('Clearing temporary license output folder');
    if (fs.existsSync(tempLicenseOutputFolder)) {
        fs.rmSync(tempLicenseOutputFolder, { recursive: true });
    }

    console.log('Completed')
}