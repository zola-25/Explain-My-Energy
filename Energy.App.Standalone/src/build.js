import { execSync } from 'child_process';
import yargs from 'yargs';
import { hideBin } from 'yargs/helpers';
import { dirname, resolve as _resolve } from 'path';
import webpack from 'webpack';
import fs from 'fs';
import { clearDirectory } from './clearDirectory.js';
import webpackDevConfig from './webpack.htmlb.dev.config.js';
import webpackProdConfig from './webpack.htmlb.prod.config.js';

const argv = yargs(hideBin(process.argv))
    .option('production',
        {
            type: 'boolean',
            describe: 'Include production build',
            default: false
        })
    .option('staging',
        {
            type: 'boolean',
            describe: 'Include staging build',
            default: false
        })
    .option('development', {
        type: 'boolean',
        description: 'Include development build',
        default: false
    })
    .option("includedemo", {
        type: 'boolean',
        describe: 'Include demo builds',
    })
    .option("demoonly", {
        type: 'boolean',
        describe: 'Only create a demo build for the specified environments',
    })
    .conflicts({
        demoonly: ['includedemo'],
        includedemo: ['demoonly']
    })
    .argv;

try {

    console.log(argv);

    if (!argv.production && !argv.development && !argv.staging) {
        console.error('Error: Please provide at least one mode');
        throw new Error('Missing mode');
    }
    if (argv.demoonly && argv.includedemo) {
        console.error('Error: Please provide only one of demoonly or includedemo');
        throw new Error('Invalid options');
    }

    const scriptDirectory = dirname(process.argv[1]);

    const wwwrootPath = _resolve(scriptDirectory, '../wwwroot');
    if (fs.existsSync(wwwrootPath)) {
        clearDirectory(wwwrootPath, 'temp');
    } else {
        fs.mkdirSync(wwwrootPath);
    }

    // can just run this once for production and it wont be any different
    process.env.APP_ENV = 'production';
    execSync('node ./src/attributionAndLicensing/GenerateCreditsDoc.js', { stdio: 'inherit', env: process.env });
    process.env.APP_ENV = undefined;

    const publishingEnvironments = [];
    if (argv.production) {
        publishingEnvironments.push({ environment: 'production', includedemo: argv.includedemo, demoonly: argv.demoonly });
    }
    if (argv.staging) {
        publishingEnvironments.push({ environment: 'staging', includedemo: argv.includedemo, demoonly: argv.demoonly });
    }
    if (argv.development) {
        publishingEnvironments.push({ environment: 'development', includedemo: argv.includedemo, demoonly: argv.demoonly });
    }

    const buildWebpackConfig = (webpackConfig) => {
        return new Promise((resolve, reject) => {
            webpack(webpackConfig, (err, stats) => {
                if (err) {
                    reject(err);
                    return;
                }
                console.log(stats.toString({ colors: true }));
                resolve();
            });
        });
    };

    (async () => {

        // we like multiple environments compiled now
        for (const { environment, includedemo, demoonly } of publishingEnvironments) {

            process.env.APP_ENV = environment;

            const demoOrNonDemoBuilds = [];
            if (includedemo) {
                demoOrNonDemoBuilds.push({ isDemo: true });
                demoOrNonDemoBuilds.push({ isDemo: false });
            } else if (demoonly) {
                demoOrNonDemoBuilds.push({ isDemo: true });
            } else {
                demoOrNonDemoBuilds.push({ isDemo: false });
            }

            for (const { isDemo } of demoOrNonDemoBuilds) {

                const webAssetsBuildFolder = _resolve(scriptDirectory, `../webAssets/webAssets${isDemo ? 'Demo' : ''}${environment.at(0).toUpperCase() + environment.slice(1)}`);
                if (fs.existsSync(webAssetsBuildFolder)) {
                    clearDirectory(webAssetsBuildFolder, 'temp');
                } else {
                    fs.mkdirSync(webAssetsBuildFolder);
                }

                let webpackConfig;

                if (environment === 'production' || environment === 'staging') {
                    webpackConfig = webpackProdConfig(environment, isDemo, webAssetsBuildFolder);
                }
                else if (environment === 'development') {
                    webpackConfig = webpackDevConfig(environment, isDemo, webAssetsBuildFolder);
                }
                else {
                    throw new Error('No mode specified');
                }

                await buildWebpackConfig(webpackConfig);

            }
        }
    })().catch((error) => {
        console.error('An error occurred:', error);
        process.exitCode = 1;
    });
} catch (error) {
    process.exitCode = 1;
    console.error(error);
}


