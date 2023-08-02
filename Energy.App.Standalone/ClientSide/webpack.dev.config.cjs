/* eslint-disable no-undef */

const path = require('path');
const LicenseWebpackPlugin = require('license-webpack-plugin').LicenseWebpackPlugin;
const tsloader = require('ts-loader')


module.exports = {
    extends: './webpack.common.config.cjs',
    entry: ['./SourceTS/index.ts'],
    mode: 'development',
    devtool: 'source-map',
    output: {
        filename: 'bundle.js',
        sourceMapFilename: 'bundle.js.map',
        pathinfo: 'verbose',
        path: path.resolve(__dirname, '../wwwroot/assets'),

    },
    module: {
        rules: [
            {
                test: /\.tsx?$/,
                use: tsloader,
                exclude: /node_modules/,
                options: {
                    configFile: "./tsconfig.compile.json"
                },
            },
            {
                test: /\.jsx?$/,
                exclude: /node_modules/,
                loader: 'babel-loader',
                options: {
                    presets: [
                        [
                            '@babel/preset-env',
                            {
                                "targets": {
                                    "browserListConfigFile": "./.browserslistrc"
                                },
                                "useBuiltIns": "usage",
                                "corejs": { "version": "3.32", "proposals": false },
                                forceAllTransforms: true,
                            }
                        ]
                    ]
                }
            }]
    },

    plugins: [
        new LicenseWebpackPlugin({
            addBanner: true,
            licenseFileOverrides: {
                '@amcharts/amcharts5': 'LICENSE'
            },
            licenseTypeOverrides: {
                '@amcharts/amcharts5': 'As specified in LICENSE file:\n'
            }

        }),

    ],
    resolve: {
        extensions: ['.tsx', '.ts', '.js'],
    },
    
}




