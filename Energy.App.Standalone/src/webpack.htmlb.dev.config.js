﻿/* eslint-disable no-undef */
/* eslint-disable @typescript-eslint/no-var-requires */

import { resolve as _resolve, dirname, } from "path";
import { fileURLToPath } from 'url';
import * as sass from 'sass'
import HtmlBundlerPlugin from 'html-bundler-webpack-plugin'

const __dirname = dirname(fileURLToPath(import.meta.url));

console.log("__dirname resolves: " + __dirname);

export default {
    resolve: {
        extensions: ['.tsx', '.ts', '.js'],
    },
    plugins: [
        new HtmlBundlerPlugin({
            // define a relative or absolute path to entry templates
            outputPath: _resolve(__dirname, '../wwwroot'),
            entry: {
                index: _resolve(__dirname, 'views/index_template.html'),

            },
            loaderOptions: {
                sources: [
                    {
                        tag: 'meta',
                        filter: (tag) => { return false }
                    },
                    {
                        tag: 'link',
                        attributes: ['href'],
                        filter:  ({ value }) => {
                            if(value.startsWith('_content') ||
                                value.startsWith('_framework') ||
                                value.startsWith('Energy.App.Standalone')){
                                    return false;
                                }
                            return true;

                        }
                    },
                    {
                        tag: 'script',
                        attributes: ['src'],
                        filter: ({ value }) => {
                            if(value.startsWith('_content') ||
                                value.startsWith('_framework') ||
                                value.startsWith('Energy.App.Standalone')){
                                    return false;
                                }
                            return true;

                        }
                    }
                ]
            },
            js: {
                filename: 'js/[name].[contenthash:8].js',
            },
            css: {
                filename: 'css/[name].[contenthash:8].css',
            }
            // OR define many templates manually
        }),


    ],
    mode: 'development',
    output: {
        path: _resolve(__dirname, '../wwwroot'),
        publicPath: '/',
    },
    module: {
        rules: [
            {
                test: /\.tsx?$/,
                use: 'ts-loader',
                exclude: /node_modules/,

            },
            {
                test: /\.(scss)$/,
                use: [
                    'css-loader',
                    'postcss-loader',
                    {
                        loader: 'sass-loader', options: {
                            implementation: sass,
                        }
                    }
                ]
            },
            {
                test: /\.(woff2|woff)$/,
                type: 'asset/resource',
                include: _resolve(__dirname, 'scss/app/fonts/'),
                generator: {
                    filename: 'fonts/[name].[contenthash:8][ext]',
                },
            },
            {
                test: /\.(woff2|woff|ttf)$/,
                type: 'asset/resource',
                include: _resolve(__dirname, 'scss/fontawesome/webfonts/'),
                generator: {
                    filename: 'webfonts/[name].[contenthash:8][ext]',
                },
            }
        ],
    },

    

};