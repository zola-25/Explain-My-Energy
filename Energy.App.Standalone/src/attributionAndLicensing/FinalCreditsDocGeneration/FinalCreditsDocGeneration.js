#!/usr/bin/env node

import Handlebars from "handlebars";

import fs from "fs";

import { marked } from "marked";
import DOMPurify from "isomorphic-dompurify";
import process from "process";
import { resolve as _resolve, dirname, basename, join, relative } from "path";
import { hideBin } from 'yargs/helpers';
import yargs from 'yargs';
import appInfoConfig from "../../appInfoConfig.js";

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
            describe: 'The path to the nuget credit partial html. Defaults to project root/src/attributionAndLicensing/generatedPartials/NugetCreditsPartial.html',
        })
    .option('npmCreditsPartialHtml',
        {
            type: 'string',
            demandOption: false,
            describe: 'The path to the npm credit partial html. Defaults to project root/src/attributionAndLicensing/generatedPartials/NpmCreditsPartial.html',
        })
    .option('generatedPartialsFolder',
        {
            type: 'string',
            demandOption: false,
            describe: 'The path to the generated partials folder. Defaults to project root/src/attributionAndLicensing/generatedPartials',
        })
    
    .argv;



const finalGeneratedCreditsHtmlDocPath = _resolve(argv.finalGeneratedCreditsHtmlDocPath);

try {
    
    const scriptName = basename(process.argv[1]);
    const scriptDirectory = dirname(process.argv[1]);
    console.log(`Running ${scriptName}, in script directory: ${scriptDirectory}`);

    
    const generatedPartialsFolder = argv.generatedPartialsFolder ? argv.generatedPartialsFolder : join(scriptDirectory, '../generatedPartials');


    const finalCreditsDocGenerationTemplate = argv.finalCreditsDocGenerationTemplate ? argv.finalCreditsDocGenerationTemplate : join(scriptDirectory, 'FinalCreditsDocGenerationTemplate.hbs');

    const projectRootPath = _resolve(argv.projectRootPath);

    const finalHtmlDocPathFromRoot = relative(projectRootPath, finalGeneratedCreditsHtmlDocPath);
    console.log('Checking if existing final credits file: %s', finalHtmlDocPathFromRoot);
    if (fs.existsSync(finalGeneratedCreditsHtmlDocPath)) {

        console.log('Removing existing final credits file: %s', finalHtmlDocPathFromRoot);
        fs.rmSync(finalGeneratedCreditsHtmlDocPath);
    }


    const nugetCreditPartialHtmlFile = argv.nugetCreditPartialHtml ? argv.nugetCreditPartialHtml : join(generatedPartialsFolder, 'NugetCreditsPartial.html');
    const npmCreditsPartialHtmlFile = argv.npmCreditsPartialHtml ? argv.npmCreditsPartialHtml : join(generatedPartialsFolder, 'NpmCreditsPartial.html');

    const nugetCreditPartialHtml = fs.readFileSync(nugetCreditPartialHtmlFile, "utf8");
    const npmCreditsPartialHtml = fs.readFileSync(npmCreditsPartialHtmlFile, "utf8");

    Handlebars.registerPartial("nugetCreditsPartial", nugetCreditPartialHtml);
    Handlebars.registerPartial("npmCreditsPartial", npmCreditsPartialHtml);

    const finalCreditsDocGenerationTemplateText = fs.readFileSync(finalCreditsDocGenerationTemplate, "utf8");

    let template = Handlebars.compile(finalCreditsDocGenerationTemplateText, {
        preventIndent: true,
        strict: true,
        
    });
    const licenseFile = appInfoConfig.resolvedLicenseFilePath;

    const licenseText = fs.readFileSync(licenseFile, "utf8");
    const unsanitizedLicenseHtml = marked.parse(licenseText, {
        gfm: true,
        breaks: true,
    });

    const sanitizedLicenseHtml = DOMPurify.sanitize(unsanitizedLicenseHtml, { USE_PROFILES: { html: true }  });

    const sanitizedFullApplicationName = DOMPurify.sanitize(appInfoConfig.fullApplicationName, { USE_PROFILES: { html: true }  });
    const sanitizedLicenseType = DOMPurify.sanitize(appInfoConfig.licenseType, { USE_PROFILES: { html: true }  });
    const sanitizedVersion = DOMPurify.sanitize(appInfoConfig.version, { USE_PROFILES: { html: true }  });

    const inputArgs = {
        emeCreditsAndLicensesCss: appInfoConfig.creditsAndLicensesCss,
        emeAppName: sanitizedFullApplicationName,
        emeLicenseType: sanitizedLicenseType,
        emeFullVersion: sanitizedVersion,
        emeFullLicenseText: sanitizedLicenseHtml
    }

    const finalGeneratedCreditsDocHtml = template(inputArgs);

    fs.writeFileSync(finalGeneratedCreditsHtmlDocPath, finalGeneratedCreditsDocHtml, "utf8");

    console.log("Success - final credits file written to: %s", finalHtmlDocPathFromRoot);
}
catch (error) {
    process.exitCode = 1;
    console.error(error);
    console.error(error.message);

}

