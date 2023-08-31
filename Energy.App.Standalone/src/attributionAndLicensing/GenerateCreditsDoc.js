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

    if (fs.existsSync(generatedPartialsFolder)) {
        fs.rmSync(generatedPartialsFolder, { recursive: true });
    }
    fs.mkdirSync(generatedPartialsFolder);

    execSync(`node src/attributionAndLicensing/NugetLicenseGeneration/NugetCreditsBuilder.js --projectCsprojPath ${projectCsprojPath} --htmlFragmentToGenerateFilePath ${generatedPartialsFolder}/NugetCreditsPartial.html`,
        { stdio: 'inherit' });

    execSync(`node src/attributionAndLicensing/NpmLicenseGeneration/NpmCreditsBuilder.js --packagesJsonFolder ${projectRootPath} --htmlFragmentToGenerateFilePath ${generatedPartialsFolder}/NpmCreditsPartial.html`,
        { stdio: 'inherit' });

    execSync(`node src/attributionAndLicensing/FinalCreditsDocGeneration/FinalCreditsDocGeneration.js --projectRootPath ${projectRootPath} --finalGeneratedCreditsHtmlDocPath ${generatedPartialsFolder}/Credits.html`,
        { stdio: 'inherit' });

    if (fs.existsSync(generatedCreditsViewsPath)) {
        fs.rmSync(generatedCreditsViewsPath)
    }

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