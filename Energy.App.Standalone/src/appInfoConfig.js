import path from 'path'
import { createRequire } from 'node:module';
const require = createRequire(import.meta.url);

const packageJsonInfo = require('../package.json')



const { license, description, version, config } = packageJsonInfo
const { fullApplicationName, licenseFilePath } = config



export default function (appEnv, appDemo) {

    const defaultHeaderData = {
        title: fullApplicationName,
        description: description,
        ogType: 'website',
        ogHeadPrefix: 'og: http://ogp.me/ns#',
    }

    let headerData;

    if (appDemo === 'true' || appDemo === true) {
        headerData = {
            ...defaultHeaderData,
            ogUrl: 'https://demo.explainmyenergy.net',
            ogImage: 'https://demo.explainmyenergy.net/images/DecemberWeather.png',
            robotsContent: 'noindex, nofollow'
        }
    } 
    else if (appEnv === 'development') {
        headerData = {
            ...defaultHeaderData,
            ogUrl: 'https://dev.explainmyenergy.net',
            ogImage: 'https://dev.explainmyenergy.net/images/DecemberWeather.png',
            robotsContent: 'noindex, nofollow'
        }
    } else if (appEnv === 'staging') {
        headerData = {
            ...defaultHeaderData,
            ogUrl: 'https://staging.explainmyenergy.net',
            ogImage: 'https://staging.explainmyenergy.net/images/DecemberWeather.png',
            robotsContent: 'noindex, nofollow'
        }
    } else if (appEnv === 'production') {
        headerData = {
            ...defaultHeaderData,
            ogUrl: 'https://explainmyenergy.net',
            ogImage: 'https://explainmyenergy.net/images/DecemberWeather.png',
            robotsContent: 'noindex, nofollow'
        }
    } else {
        throw new Error('No APP_ENV environment variable set');
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
