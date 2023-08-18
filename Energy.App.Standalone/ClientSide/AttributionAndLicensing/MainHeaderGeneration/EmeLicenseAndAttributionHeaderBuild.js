#!/usr/bin/env node
import parseArgs from "minimist";
import Handlebars from "handlebars";

import fs from "fs";

import { marked } from "marked";
import DOMPurify from "isomorphic-dompurify";
import process from "process";

try {

    //console.log(process.argv);
    const args = parseArgs(process.argv.slice(2), {
        string: ["o", "l", "t", "v"]
    });
    const outputFile = args.o;
    const licenseFile = args.l;
    const templateFile = args.t;
    const version = args.v;
    
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
        emeLicenseType: "Apache 2.0",
        emeFullVersion: version,
        emeFullLicenseText: licenseHtml
    }

    const outputHtml = template(inputArgs);

    fs.writeFileSync(outputFile, outputHtml, "utf8");

}
catch (error) {
    console.error(error);
}

console.log("Success")

