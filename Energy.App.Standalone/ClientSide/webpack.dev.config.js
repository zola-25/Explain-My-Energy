/* eslint-disable no-undef */
/* eslint-disable @typescript-eslint/no-var-requires */

const path = require('path');
const LicenseWebpackPlugin = require('license-webpack-plugin').LicenseWebpackPlugin;
const HtmlWebpackPlugin = require('html-webpack-plugin');

module.exports = {
    entry: './index.ts',
    mode: 'development',
    output: {
        filename: 'bundle.[contenthash].js',
        sourceMapFilename: '[file].map',
        path: path.resolve(__dirname, '../wwwroot/js'),
        
        clean: true
    },
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
            skipChildCompilers: true,
            
            outputFilename: 'licenses.txt',
            addBanner: true,
            licenseFileOverrides: {
                '@amcharts/amcharts5': 'LICENSE'
            },
            licenseTypeOverrides: {
                '@amcharts/amcharts5': 'As specified in LICENSE file:\n'
            }

        }),
        new HtmlWebpackPlugin({
            template: './HtmlTemplates/index_template.html',
            filename: '../index.html',
            inject: false,
            minify: false
        }), 
        
    ],
    resolve: {
        extensions: ['.tsx', '.ts', '.js'],
    },

};