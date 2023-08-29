import path from 'path'
import {license, description,version,config} from '../package.json'

const {fullApplicationName, licenseFilePath} = config

const resolvedLicenseFilePath = path.resolve("..", licenseFilePath)

const appInfoConfig = {
    fullApplicationName,
    licenseType: license,
    description,
    version,
    resolvedLicenseFilePath
}


export default appInfoConfig