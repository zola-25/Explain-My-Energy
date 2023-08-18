/* eslint-disable no-undef */
/* eslint-disable @typescript-eslint/no-var-requires */

import HtmlWebpackPlugin from "html-webpack-plugin";
import { resolve as _resolve, dirname } from "path";
import { fileURLToPath } from 'url';

const __dirname = dirname(fileURLToPath(import.meta.url));

export default {
    entry: './index.ts',
    mode: 'development',
    output: {
        filename: 'bundle.[contenthash].js',
        sourceMapFilename: '[file].map',
        path: _resolve(__dirname, '../wwwroot/js'),
        
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