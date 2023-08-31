import { execSync } from 'child_process';
import { createRequire } from 'node:module';
import yargs from 'yargs';
import { hideBin } from 'yargs/helpers';
import { dirname, _resolve as resolve} from 'path';
import webpack from 'webpack';
import clearDirectory from './clearDirectory.js';
import  webpackDevConfig from './webpack.htmlb.dev.config.js';
import webpackProdConfig from './webpack.htmlb.prod.config.js';


const argv = yargs(hideBin(process.argv))
    .option('production',
        {
            type: 'boolean',
            describe: 'Run in production mode',
        })
        .option('development',{
            type: 'boolean',
            description: 'Run in development mode',

        })
        .conflicts('development', 'production')
        .demandOption(['development', 'production'], 'Please provide at least one mode')
    .argv;


try {
    const scriptDirectory = dirname(process.argv[1]);

    execSync('node ./src/attributionAndLicensing/GenerateCreditsDoc.js', { stdio: 'inherit' });

    const wwwrootPath = _resolve(scriptDirectory, '../wwwroot');

    clearDirectory(wwwrootPath, 'temp');

    let webpackConfig;

    if (process.argv.production) {
        webpackConfig = webpackProdConfig;
    } else if (process.argv.development) {
        webpackConfig = webpackDevConfig;
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


