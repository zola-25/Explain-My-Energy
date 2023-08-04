/* eslint-disable @typescript-eslint/no-var-requires */
/*eslint no-undef: "error"*/
/*eslint-env node*/

const HtmlWebpackPlugin = require("html-webpack-plugin");
const path = require("path");
const TerserPlugin = require("terser-webpack-plugin");
const LicenseWebpackPlugin = require('license-webpack-plugin').LicenseWebpackPlugin;


module.exports = {
    entry: './index.ts',
    mode: 'production',
    output: {
        filename: 'bundle.[contenthash].js',
        path: path.resolve(__dirname, '../wwwroot/js'),
        
        clean: true,
    },
    devtool: 'hidden-source-map',
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

            outputFilename: '../licenses.txt',
            addBanner: true,
            licenseFileOverrides: {
                '@amcharts/amcharts5': 'LICENSE'
            },
            licenseTypeOverrides: {
                '@amcharts/amcharts5': 'As specified in LICENSE file:\n'
            },
            renderBanner: (filename) => {
                return '/*! licenses are at ' + filename + '*/';
            }
        }),
        new HtmlWebpackPlugin({
            template: './HtmlTemplates/index_template.html',
            filename: '../index.html',
            inject: false,
            minify: false,
               

        }), 
    ],
    optimization: {
        minimize: true,
        
        minimizer: [new TerserPlugin({
            extractComments: false,
            terserOptions: {
                sourceMap: true,
                format: {
                    // Tell terser to remove all comments except for the banner added via LicenseWebpackPlugin.
                    // This can be customized further to allow other types of comments to show up in the final js file as well.
                    // See the terser documentation for format.comments options for more details.
                    comments: (astNode, comment) => comment.value.startsWith('! licenses are at ')
                        
                }

            },
            
        })],
    },
    resolve: {
        extensions: ['.tsx', '.ts', '.js'],
    }
};
