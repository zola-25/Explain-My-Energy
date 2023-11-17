import path from 'path'
import { createRequire } from 'node:module';
import dotenv from 'dotenv';

const require = createRequire(import.meta.url);

const packageJsonInfo = require('../package.json')
dotenv.config({ path: path.resolve(process.cwd(), "src", '.env')});

const { license, description, version, config } = packageJsonInfo
const { fullApplicationName, licenseFilePath } = config

const instrumentationKey = process.env.APPINSIGHTS_INSTRUMENTATIONKEY;

export default function (appEnv, appDemo) {

    const defaultHeaderData = {
        title: fullApplicationName,
        description: description,
        ogType: 'website',
        ogHeadPrefix: 'og: http://ogp.me/ns#',
        instrumentationKey: instrumentationKey,
    }

    let headerData;

    if (appDemo === 'true' || appDemo === true) {
        headerData = {
            ...defaultHeaderData,
            ogUrl: 'https://demo.explainmyenergy.net',
            ogImage: 'https://demo.explainmyenergy.net/images/DecemberWeather.png',
        }

    } 
    else {
        headerData = {
            ...defaultHeaderData,
            ogUrl: 'https://explainmyenergy.net',
            ogImage: 'https://explainmyenergy.net/images/DecemberWeather.png',
        }
    } 
    if (appEnv !== 'production') {
        headerData.robotsContent = 'noindex, nofollow'
    }

    const resolvedLicenseFilePath = path.resolve(licenseFilePath)

    const appInfoConfig = {
        creditsAndLicensesCss: '../scss/app/attribs.scss',
        fullApplicationName: fullApplicationName,
        licenseType: license,
        description: description,
        version: version,
        resolvedLicenseFilePath: resolvedLicenseFilePath,
        projectRootPath: path.resolve('.'),
        generatedPartialsOutputDirectory: path.resolve('./src/attributionAndLicensing/generatedPartials'),
        headerData: headerData
    }
    return appInfoConfig;
}
