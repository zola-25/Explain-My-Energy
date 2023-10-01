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
            describe: 'Run in production mode',
        })
    .option('staging',
        {
            type: 'boolean',
            describe: 'Run in staging mode',
        })
    .option('development', {
        type: 'boolean',
        description: 'Run in development mode',
    })
    .conflicts({
        development: ['staging', 'production'],
        staging: ['development', 'production'],
        production: ['development', 'staging']
    })
    .option("demo", {
        type: 'boolean',
        describe: 'Run in demo mode',
        default: false
    })
    .argv;


try {

    console.log(argv);

    if (!argv.production && !argv.development && !argv.staging) {
        console.error('Error: Please provide at least one mode');
        throw new Error('Missing mode');
    }

    if (argv.production) {
        process.env.APP_ENV = 'production';
    } else if (argv.staging) {
        process.env.APP_ENV = 'staging';
    } else if (argv.development) {
        process.env.APP_ENV = 'development';
    } else {
        throw new Error('No mode specified');
    }

    if (argv.demo) {
        process.env.APP_DEMO = 'true'
    }

    const scriptDirectory = dirname(process.argv[1]);

    execSync('node ./src/attributionAndLicensing/GenerateCreditsDoc.js', { stdio: 'inherit', env: process.env });

    const wwwrootPath = _resolve(scriptDirectory, '../wwwroot');

    if (fs.existsSync(wwwrootPath)) {
        clearDirectory(wwwrootPath, 'temp');
    }

    let webpackConfig;

    if (argv.production) {
        webpackConfig = webpackProdConfig('production', argv.demo);
    } else if (argv.staging) {
        webpackConfig = webpackProdConfig('staging', argv.demo);
    } else if (argv.development) {
        webpackConfig = webpackDevConfig('development', argv.demo);
    } else {
        throw new Error('No mode specified');
    }

    webpack(webpackConfig, (err, stats) => {
        if (err) {
            console.error(err);
            return;
        }

        console.log(stats.toString({
            colors: true
        }));
    });
}
catch (error) {
    process.exitCode = 1;
    console.error(error);
}


