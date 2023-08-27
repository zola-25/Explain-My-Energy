#!/usr/bin/env node
import parseArgs from "minimist";
import Handlebars from "handlebars";

import fs from "fs";

import { marked } from "marked";
import DOMPurify from "isomorphic-dompurify";
import process from "process";
import { resolve as _resolve, dirname, posix } from "path";
import { fileURLToPath } from 'url';

const __dirname = dirname(fileURLToPath(import.meta.url));

console.log("__dirname resolves: " + __dirname);

try {

    //console.log(process.argv);
    const args = parseArgs(process.argv.slice(2), {
        string: ["o"]
    }); 
    const outputFile = args.o;

    const templateFile = _resolve(__dirname, 'EmeLicenseAndAttributionHeaderTemplate.hbs');
    
    const attribsCssPath = process.env.npm_config_attribsCssPath;
    const version = process.env.npm_package_version;
    const fullApplicationName = process.env.npm_config_fullApplicationName;
    const licenseType = process.env.npm_package_license;
    const licenseFile = process.env.npm_config_licenseFile;

    const licenseText = fs.readFileSync(licenseFile, "utf8");

    const templateText = fs.readFileSync(templateFile, "utf8");

    const unsanitizedHtml = marked.parse(licenseText, {
        gfm: true,
        breaks: true,
    });

    const licenseHtml = DOMPurify.sanitize(unsanitizedHtml);

    let template = Handlebars.compile(templateText, {
        preventIndent: true,
        strict: true,
        noEscape: true,
    });

    const inputArgs = {
        emeAttribsCssPath: attribsCssPath,
        emeAppName: fullApplicationName,
        emeLicenseType: licenseType,
        emeFullVersion: version,
        emeFullLicenseText: licenseHtml
    }

    const outputHtml = template(inputArgs);

    fs.writeFileSync(outputFile, outputHtml, "utf8");

    console.log("Success")
}
catch (error) {
    console.error(error);
    process.exitCode = 1;
}


