/* eslint-disable @typescript-eslint/no-var-requires */
/*eslint no-undef: "error"*/
/*eslint-env node*/

import HtmlWebpackPlugin from "html-webpack-plugin";
import { resolve as _resolve, dirname } from "path";
import TerserPlugin from "terser-webpack-plugin";
import { fileURLToPath } from 'url';

const __dirname = dirname(fileURLToPath(import.meta.url));

export default {
    entry: './index.ts',
    mode: 'production',
    output: {
        filename: 'bundle.[contenthash].js',
        path: _resolve(__dirname, '../wwwroot/js'),
        
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
            extractComments: "false",
            terserOptions: {
                sourceMap: true,
                format: {
                    // See the terser documentation for format.comments options for more details.
                    comments: "some"
                },
            },
        })],
    },
    resolve: {
        extensions: ['.tsx', '.ts', '.js'],
    }
};
