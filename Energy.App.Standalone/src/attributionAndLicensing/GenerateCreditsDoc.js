#!/usr/bin/env node
import { execSync } from 'child_process';
import fs from 'fs';
import path from 'path';
import appInfoConfig from '../appInfoConfig.js';

const { projectRootPath, generatedPartialsOutputDirectory } = appInfoConfig;
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
    execSync(`node src/attributionAndLicensing/NugetLicenseGeneration/NugetCreditsBuilder.js --projectCsprojPath ${projectCsprojPath} --htmlFragmentToGenerateFilePath ${generatedPartialsFolder}/NugetCreditsPartial.html`,
        { stdio: 'inherit' });

    console.log('Generating Npm credits partial')
    execSync(`node src/attributionAndLicensing/NpmLicenseGeneration/NpmCreditsBuilder.js --packagesJsonFolder ${projectRootPath} --htmlFragmentToGenerateFilePath ${generatedPartialsFolder}/NpmCreditsPartial.html`,
        { stdio: 'inherit' });

    console.log('Generating final credits file')
    execSync(`node src/attributionAndLicensing/FinalCreditsDocGeneration/FinalCreditsDocGeneration.js --projectRootPath ${projectRootPath} --finalGeneratedCreditsHtmlDocPath ${generatedPartialsFolder}/Credits.html`,
        { stdio: 'inherit' });

    

    console.log('Copying generated credits file to views folder')
    fs.copyFileSync(path.resolve(generatedPartialsFolder, 'Credits.html'), generatedCreditsViewsPath);

    console.log('Successfully generated credits file in views folder')

} catch (error) {

    console.log('Error generating credits file in views folder')
    console.log(error);
    console.log(error.message);

    process.exitCode = 1;
}
finally {

    console.log('Cleaning up generated partials folder')
    if (fs.existsSync(generatedPartialsFolder)) {
        fs.rmSync(generatedPartialsFolder, { recursive: true });
    }

}