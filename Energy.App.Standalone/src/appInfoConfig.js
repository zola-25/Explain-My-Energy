import path from 'path'
import { createRequire } from 'node:module';
const require = createRequire(import.meta.url);

const appInfo = require('../package.json')

const { license, description, version, config } = appInfo
const {fullApplicationName, licenseFilePath} = config

const resolvedLicenseFilePath = path.resolve(licenseFilePath)

const appInfoConfig = {
    creditsAndLicensesCss: '../scss/app/attribs.scss',
    fullApplicationName: fullApplicationName,
    licenseType: license,
    description: description,
    version: version,
    resolvedLicenseFilePath: resolvedLicenseFilePath
}


export default appInfoConfig