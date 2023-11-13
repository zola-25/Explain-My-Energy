#!/usr/bin/env node
import { execFileSync, execSync } from 'child_process';
import fs from 'fs';
import path from 'path';
import appInfoConfig from '../appInfoConfig.js';

const APP_ENV = process.env.APP_ENV;

const { projectRootPath, generatedPartialsOutputDirectory } = appInfoConfig(APP_ENV, 'false');
const generatedPartialsFolder = path.resolve(generatedPartialsOutputDirectory)
const generatedCreditsViewsPath = path.resolve(projectRootPath, 'src/views/Credits.html')

try {
    const projectCsprojPath = path.resolve(projectRootPath, 'Energy.App.Standalone.csproj');

    console.log('Cleaning up generated credits file in views folder')
    if (fs.existsSync(generatedCreditsViewsPath)) {
        fs.rmSync(generatedCreditsViewsPath)
    }

    if (fs.existsSync(generatedPartialsFolder)) {
        console.log('Cleaning up generated partials folder')
        fs.rmSync(generatedPartialsFolder, { recursive: true });
    }

    console.log('Recreating generated partials folder')
    fs.mkdirSync(generatedPartialsFolder);

    console.log('Generating Nuget credits partial')

    const scriptDirectory = path.dirname(process.argv[1]);
    const nugetCreditsBuilderPath = path.resolve(scriptDirectory, './NugetLicenseGeneration/NugetCreditsBuilder.js');
    const nugetCmd = "node" //./NugetLicenseGeneration/NugetCreditsBuilder.js";
    const nugetArgs = [nugetCreditsBuilderPath, "--projectCsprojPath", projectCsprojPath, "--htmlFragmentToGenerateFilePath", path.join(generatedPartialsFolder, "NugetCreditsPartial.html")];
    const nugetResult = execFileSync(nugetCmd, nugetArgs, { shell: false, stdio: ['inherit', 'inherit', 'pipe'],  env: process.env });

    if(nugetResult && nugetResult.stderr) {
        console.log(nugetResult.stderr.toString());
        throw new Error("Error generating Nuget credits partial", nugetResult.stderr.toString());
    }
    console.log('Generating Npm credits partial')

    const npmCreditsBuilderPath = path.resolve(scriptDirectory, './NpmLicenseGeneration/NpmCreditsBuilder.js');
    const npmArgs = [npmCreditsBuilderPath, "--packagesJsonFolder", projectRootPath, "--htmlFragmentToGenerateFilePath", path.join(generatedPartialsFolder, "NpmCreditsPartial.html")];
    execFileSync("node", npmArgs, { shell: false, stdio: 'inherit', env: process.env });


    console.log('Generating final credits file')

    const finalCreditsBuilderPath = path.resolve(scriptDirectory, './FinalCreditsDocGeneration/FinalCreditsDocGeneration.js');
    const finalCreditsArgs = [finalCreditsBuilderPath, "--projectRootPath", projectRootPath, "--finalGeneratedCreditsHtmlDocPath", path.join(generatedPartialsFolder, "Credits.html")];
    
    const finalCreditsResult = execFileSync("node", finalCreditsArgs, { shell: false, stdio: ['inherit', 'inherit', 'pipe'], env: process.env});
    
    if(finalCreditsResult && finalCreditsResult.stderr) {
        console.log(finalCreditsResult.stderr.toString());
        throw new Error("Error generating final credits file", finalCreditsResult.stderr.toString());
    }

    console.log('Copying generated credits file to views folder')
    fs.copyFileSync(path.resolve(generatedPartialsFolder, 'Credits.html'), generatedCreditsViewsPath);

    console.log('Successfully generated credits file in views folder')

} catch (error) {

    process.exitCode = 1;

    console.log('Error generating credits file in views folder')
    console.log(error);
    console.log(error.message);

}
finally {

    console.log('Cleaning up generated partials folder')
    if (fs.existsSync(generatedPartialsFolder)) {
        fs.rmSync(generatedPartialsFolder, { recursive: true });
    }

}