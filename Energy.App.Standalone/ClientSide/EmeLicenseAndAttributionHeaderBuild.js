#!/usr/bin/env node
import parseArgs from "minimist";
import Handlebars from "handlebars";

import fs from "fs";

import path from "path";
import { fileURLToPath } from 'url';

import { marked } from "marked";
import DOMPurify from "isomorphic-dompurify";
import process from "process";

try {

    //console.log(process.argv);
    const args = parseArgs(process.argv.slice(2), {
        string: "o"
    });
    const outputFile = args.o;

    /* const outputFile = "C:/Users/miket/Development/ExplainMyEnergy/Energy.App.Standalone/ClientSide/tempLicenseHtmlOutput/LicenseAndAttributionHeader.html" */
    const __dirname = path.dirname(fileURLToPath(import.meta.url));

    const licenseFile = path.join(__dirname, "../../LICENSE");
    const licenseText = fs.readFileSync(licenseFile, "utf8");

    const templateFile = path.join(__dirname, "./HtmlTemplates/EmeLicenseAndAttributionHeader.handlebars");
    const templateText = fs.readFileSync(templateFile, "utf8");

    const unsanitizedHtml = marked.parse(licenseText, {
        gfm: true,
        breaks: true,
    });

    const licenseHtml = DOMPurify.sanitize(unsanitizedHtml);

    let template = Handlebars.compile(templateText, {
        preventIndent: true,
        strict: true
    });

    const inputArgs = {
        emeLicenseType: "Apache 2.0",
        emeFullVersion: "0.1.0",
        emeFullLicenseText: licenseHtml
    }

    const outputHtml = template(inputArgs);

    fs.writeFileSync(outputFile, outputHtml, "utf8");

}
catch (error) {
    console.error(error);
}

console.log("Success")

