#!/usr/bin/env node

import Handlebars from "handlebars";

import fs from "fs";

import { marked } from "marked";
import DOMPurify from "isomorphic-dompurify";
import process from "process";
import { resolve as _resolve, dirname, basename, join, relative } from "path";
import { hideBin } from 'yargs/helpers';
import yargs from 'yargs';


const argv = yargs(hideBin(process.argv))
    .option('projectRootPath',
        {
            type: 'string',
            demandOption: true,
            describe: 'The path to the application root folder.',
        })
    .option('finalGeneratedCreditsHtmlDocPath',
        {
            type: 'string',
            demandOption: true,
            describe: 'The path to the final generated credits html document.',
        })

    .option('finalCreditsDocGenerationTemplate',
        {
            type: 'string',
            demandOption: false,
            describe: 'The path to the final credits consolidate template. Defaults to the current script directory/FinalCreditsDocGenerationTemplate.hbs',
        })
    .option('nugetCreditPartialHtml',
        {
            type: 'string',
            demandOption: false,
            describe: 'The path to the nuget credit partial html. Defaults to project root/src/views/NugetCreditsPartial.html',
        })
    .option('npmCreditsPartialHtml',
        {
            type: 'string',
            demandOption: false,
            describe: 'The path to the npm credit partial html. Defaults to project root/src/views/NpmCreditsPartial.html',
        })
    .option('viewsFolder',
        {
            type: 'string',
            demandOption: false,
            describe: 'The path to the views folder. Defaults to project root/src/views',
        })
    .argv;

const scriptName = basename(process.argv[1]);
const scriptDirectory = dirname(process.argv[1]);
console.log(`${scriptName} Script directory: ${scriptDirectory}`);

const finalGeneratedCreditsHtmlDocPath = _resolve(argv.finalGeneratedCreditsHtmlDocPath);
const finalCreditsDocGenerationTemplate = argv.finalCreditsDocGenerationTemplate ? argv.finalCreditsDocGenerationTemplate : join(scriptDirectory, 'FinalCreditsDocGenerationTemplate.hbs');

const projectRootPath = _resolve(argv.projectRootPath);
const viewsFolder = argv.viewsFolder ? argv.viewsFolder : join(projectRootPath, 'src/views');
const nugetCreditPartialHtmlFile = argv.nugetCreditPartialHtml ? argv.nugetCreditPartialHtml : join(viewsFolder, 'NugetCreditsPartial.html');
const npmCreditsPartialHtmlFile = argv.npmCreditsPartialHtml ? argv.npmCreditsPartialHtml : join(viewsFolder, 'NpmCreditsPartial.html');

try {

    const attribsCssPath = process.env.npm_package_config_attribsCssPath;
    const appVersion = process.env.npm_package_version;
    const fullApplicationName = process.env.npm_package_config_fullApplicationName;
    const licenseType = process.env.npm_package_license;
    const licenseFile = _resolve(process.env.npm_package_config_licenseFile);

    const licenseText = fs.readFileSync(licenseFile, "utf8");
    const unsanitizedLicenseHtml = marked.parse(licenseText, {
        gfm: true,
        breaks: true,
    });

    const sanitizedLicenseHtml = DOMPurify.sanitize(unsanitizedLicenseHtml);

    const nugetCreditPartialHtml = fs.readFileSync(nugetCreditPartialHtmlFile, "utf8");
    const npmCreditsPartialHtml = fs.readFileSync(npmCreditsPartialHtmlFile, "utf8");

    Handlebars.registerPartial("nugetCreditsPartial", nugetCreditPartialHtml);
    Handlebars.registerPartial("npmCreditsPartial", npmCreditsPartialHtml);

    const finalCreditsDocGenerationTemplateText = fs.readFileSync(finalCreditsDocGenerationTemplate, "utf8");



    let template = Handlebars.compile(finalCreditsDocGenerationTemplateText, {
        preventIndent: true,
        strict: true,
        noEscape: true,
    });

    const inputArgs = {
        emeAttribsCssPath: attribsCssPath,
        emeAppName: fullApplicationName,
        emeLicenseType: licenseType,
        emeFullVersion: appVersion,
        emeFullLicenseText: sanitizedLicenseHtml
    }

    const finalGeneratedCreditsDocHtml = template(inputArgs);

    const sanitizedFinalGeneratedCreditsDocHtml = DOMPurify.sanitize(finalGeneratedCreditsDocHtml, { html: true });

    console.log('Final credits file check: DOMPurify removed %d elements.', DOMPurify.removed.length)

    if (DOMPurify.removed.length > 0) {
        console.log('Elements removed:  %s', JSON.stringify(DOMPurify.removed));
    }

    fs.writeFileSync(finalGeneratedCreditsHtmlDocPath, sanitizedFinalGeneratedCreditsDocHtml, "utf8");

    const docOutputPath = relative(projectRootPath, finalGeneratedCreditsHtmlDocPath);
    console.log("Success - final credits file written to: %s", docOutputPath);
}
catch (error) {
    console.error(error);
    process.exitCode = 1;
}


