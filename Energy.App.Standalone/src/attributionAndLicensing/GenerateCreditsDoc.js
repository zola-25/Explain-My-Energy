#!/usr/bin/env node
import { execSync } from 'child_process';
import fs from 'fs';
import path from 'path';
import appInfoConfig from '../appInfoConfig';

const { projectRootPath, generatedPartialsOutputDirectory } = appInfoConfig;
const generatedPartialsFolder = path.resolve(generatedPartialsOutputDirectory)


try{
    const projectCsprojPath = path.resolve(projectRootPath, 'Energy.App.Standalone.csproj');

    if (fs.existsSync(generatedPartialsFolder)) {
        fs.rmSync(generatedPartialsFolder, { recursive: true });
    }
    fs.mkdirSync(generatedPartialsFolder);

    execSync(`node NugetLicenseGeneration/NugetCreditsBuilder.js --projectCsprojPath ${projectCsprojPath} --generatedHtmlDocumentPath ${generatedPartialsFolder}/NugetCreditsPartial.html`,
        { stdio: 'inherit' });

    execSync(`node NpmLicenseGeneration/NpmCreditsBuilder.js --packagesJsonFolder ${projectRootPath} --generatedHtmlDocumentPath ${generatedPartialsFolder}/NpmCreditsPartial.html`,
        { stdio: 'inherit' });

    execSync(`node FinalCreditsDocGeneration/FinalCreditsDocGeneration.js --projectRootPath ${projectRootPath} --finalGeneratedCreditsHtmlDocPath ${generatedPartialsFolder}/Credits.html`, 
        { stdio: 'inherit' });

    fs.copyFileSync(path.resolve(generatedPartialsFolder, 'Credits.html'), path.resolve(projectRootPath, 'src/views/Credits.html'));

} catch (error) {

    if (fs.existsSync(generatedPartialsFolder)) {
        fs.rmSync(generatedPartialsFolder, { recursive: true });
    }

    console.log(error);
    process.exitCode = 1;
}