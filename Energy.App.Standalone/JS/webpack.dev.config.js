/* eslint-disable no-undef */
/* eslint-disable @typescript-eslint/no-var-requires */

const path = require('path');
const LicenseWebpackPlugin = require('license-webpack-plugin').LicenseWebpackPlugin;
const fs = require('fs');

module.exports = {
    entry: './index.ts',
    mode: 'development',
    devtool: 'source-map',
    module: {
        rules: [
            {
                test: /\.tsx?$/,
                use: 'ts-loader',
                exclude: /node_modules/,
            },
        ],
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

        })
    ],
    resolve: {
        extensions: ['.tsx', '.ts', '.js'],
    },
    output: {
        filename: 'bundle.js',
        sourceMapFilename: 'bundle.js.map',
        path: path.resolve(__dirname, '../wwwroot/js'),
        clean: true
    },
};