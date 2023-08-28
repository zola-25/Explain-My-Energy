import { execSync } from 'child_process';
import fs from 'fs';
import path from 'path';
import yargs from 'yargs';
import { hideBin } from 'yargs/helpers';
import he from 'he';
import DOMPurify from "isomorphic-dompurify";
import Handlebars from "handlebars";
import { marked } from 'marked';


const argv = yargs(hideBin(process.argv))
.option('clientSideRootPath', {    
    type: 'string',
    demandOption: true,
    describe: 'The folder containing the projects packages.json file',
}).option('generatedHtmlDocumentPath', {
    type: 'string',
    demandOption: false,
    describe: 'The path to the generated html document. Defaults to the script directory/NpmCreditsPartial.html',
}).option('tempLicenseOutputFolder', {
    type: 'string',
    demandOption: false,
    describe: 'The path to the temporary license output folder where the license generation will take place. Defaults to the script directory/NpmLicenseOutput',
}).option('customFormatFile', {
    type: 'string',
    demandOption: false,
    describe: 'The path to the license customFormat JSON file. Defaults to the script directory/customFormatExample.json',
}).option('npmCreditsTemplateFile', {
    type: 'string',
    demandOption: false,
    describe: 'The path to the npm credits template file. Defaults to the script directory/NpmCreditsPartialTemplate.hbs',
}).argv;

const scriptDirectory = path.dirname(process.argv[1]);
console.log(`Script directory: ${scriptDirectory}`); 

const clientSideRootPath = path.resolve(argv.clientSideRootPath);

const generatedHtmlDocumentPath = argv.generatedHtmlDocumentPath ? argv.generatedHtmlDocumentPath : path.join(scriptDirectory, './NpmCreditsPartial.html');
const customFormatFile = argv.customFormatFile ? argv.customFormatFile : path.join(scriptDirectory, './customFormatExample.json');
const tempLicenseOutputFolder = argv.tempLicenseOutputFolder ? argv.tempLicenseOutputFolder : path.join(scriptDirectory, './NpmLicenseOutput');
const npmCreditsTemplateFile = argv.npmCreditsTemplateFile ? argv.npmCreditsTemplateFile : path.join(scriptDirectory, './NpmCreditsPartialTemplate.hbs');

const originalLocation = process.cwd();

try {

    if (fs.existsSync(tempLicenseOutputFolder)) {
        fs.rmSync(tempLicenseOutputFolder, { recursive: true });
    }
    fs.mkdirSync(tempLicenseOutputFolder);


    const packageLicenceJsonFile = path.join(tempLicenseOutputFolder, './NpmLicenses.json');
    const licensePlainTextFolder = path.join(tempLicenseOutputFolder, './NpmLicensePlainTextFiles');

    fs.mkdirSync(licensePlainTextFolder);

    process.chdir(clientSideRootPath);

    execSync(`license-checker-rseidelsohn --production --json --nopeer --excludePackagesStartingWith="explain-my-energy" \
            --out ${packageLicenceJsonFile} --excludePrivatePackages --relativeModulePath --relativeLicensePath \
            --files ${licensePlainTextFolder} --customPath ${customFormatFile}`);

    process.chdir(originalLocation);

    const rawJson = fs.readFileSync(packageLicenceJsonFile, 'utf8');
    const packageInfos = JSON.parse(rawJson);

    const licenseHashRegEx = /(?<header>^#+\s+licen[sc]es?\s*)(?<license>.+?(?=#+|$(?![\r\n])))/gims
    const licenseUnderlineHeaderRegEx = /(?<header>^\s*licen[sc]es?.*\n[=-]+\n)(?<license>.+?(?==+|-{2,}|$(?![\r\n])))/gims
    
    const encodedPackageInfosWithLicenseText = [];

    for (const [packageNameAndVersion, packageDetails] of Object.entries(packageInfos)) {

        const packageName = packageDetails.name;
        console.log("Processing package", packageName);

        const licenceTextFile = path.join(tempLicenseOutputFolder, packageDetails.licenseFile);
        const licensePlainText = fs.existsSync(licenceTextFile)
            ? fs.readFileSync(licenceTextFile, 'utf8')
            : (() => { throw new Error(`License text file not found: ${licenceTextFile}`); })();


            
        let licenseContent;

        const hashResultRegEx = licenseHashRegEx.exec(licensePlainText)

        if (hashResultRegEx && hashResultRegEx.groups) {
            licenseContent = hashResultRegEx.groups.license;
        } else {
            const underlineResultRegEx = licenseUnderlineHeaderRegEx.exec(licensePlainText)

            if (underlineResultRegEx && underlineResultRegEx.groups) {
                licenseContent = underlineResultRegEx.groups.license;
            } else {
                licenseContent = licensePlainText;
            }
        }

        const licensePlainTextEncoded = marked.parse(licenseContent, {
            gfm: true,
            breaks: true,
        });

        const licensePlainTextEncodedSanitized = DOMPurify.sanitize(licensePlainTextEncoded, { USE_PROFILES: { html: true }});


        const lowerCaseLicenseContent = licenseContent.toLowerCase();
        const licenseHasCopyrightText = lowerCaseLicenseContent.includes("copyright") || lowerCaseLicenseContent.includes("copy right");
        const licenseHasCopyrightSymbol = ["©", "Ⓒ", "&copy;", "&COPY;", "&#169;", "&#xA9;", "&#9400;", "&#x24B8;"]
            .some(symbol => licenseContent.includes(symbol));

        const hasCopyrightInLicense = licenseHasCopyrightText || licenseHasCopyrightSymbol;

        let copyrightHtmlSanitized = "";
        if (!hasCopyrightInLicense && packageDetails.copyright.trim()) {
            const copyrightHtml = marked.parse(packageDetails.copyright, {
                gfm: true,
                breaks: true,
            })

            copyrightHtmlSanitized = DOMPurify.sanitize(copyrightHtml, { USE_PROFILES: { html: true }});
        }
        

        if (!packageDetails.licenses.trim()) {
            throw new Error(`License is null or whitespace for package ${packageNameAndVersion}`);
        }

        if (packageDetails.licenses.split(",").length > 1) {
            throw new Error(`Multiple licenses found for package ${packageNameAndVersion}`);
        }

        const licenseTypeEncoded = he.encode(packageDetails.licenses.trim(), { strict: true });
        const licenseTypeEncodedSanitized = DOMPurify.sanitize(licenseTypeEncoded, { USE_PROFILES: { html: true }});

        const packageNameEncoded = he.encode(packageName, { strict: true });
        const packageNameEncodedSanitized = DOMPurify.sanitize(packageNameEncoded, { USE_PROFILES: { html: true }});

        const hasSeparateCopyright = !!copyrightHtmlSanitized; 

        const encodedPackageInfo = { 
            PackageName: packageNameEncodedSanitized, 
            HasSeparateCopyright: hasSeparateCopyright,
            Copyright: copyrightHtmlSanitized, 
            LicenseTextEncodedToInsert: licensePlainTextEncodedSanitized,
            LicenseType: licenseTypeEncodedSanitized };

        encodedPackageInfosWithLicenseText.push(encodedPackageInfo);

    }

    const templateText = fs.readFileSync(npmCreditsTemplateFile, 'utf8');

    const template = Handlebars.compile(templateText, { preventIndent: true, strict: true, noEscape: true });
    const htmlRendered = template({ packageInfos: encodedPackageInfosWithLicenseText });

    const htmlSanitized = DOMPurify.sanitize(htmlRendered, { USE_PROFILES: { html: true } });

    console.log('DOMPurify removed %d elements.', DOMPurify.removed.length)

    if (DOMPurify.removed.length > 0) {
        console.log('Elements removed:  %s', JSON.stringify(DOMPurify.removed));
    }

    fs.writeFileSync(generatedHtmlDocumentPath, htmlSanitized, 'utf8');



} catch  (error) {
    console.error(error);
    console.error(error.message);
    console.error(error.stack);
    process.exitCode = 1;
}
finally{

    process.chdir(originalLocation);

    console.log('Clearing temporary license output folder');

    if(fs.existsSync(tempLicenseOutputFolder)){
        fs.rmSync(tempLicenseOutputFolder, { recursive: true });
    }

    console.log('Completed')

}
