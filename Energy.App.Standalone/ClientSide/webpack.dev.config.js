/* eslint-disable no-undef */
/* eslint-disable @typescript-eslint/no-var-requires */

import HtmlWebpackPlugin from "html-webpack-plugin";
import MiniCssExtractPlugin from "mini-css-extract-plugin";
import { resolve as _resolve, dirname } from "path";
import { fileURLToPath } from 'url';
import * as sass from 'sass'


const __dirname = dirname(fileURLToPath(import.meta.url));

console.log("__dirname resolves: " + __dirname);

export default {
    entry: {
        main: './ClientSide/index.ts',
        styles: './ClientSide/Sass/app.scss'
    },
    mode: 'development',
    output: {
        filename: 'js/[name].[contenthash].js',
        sourceMapFilename: '[file].map',
        path: _resolve(__dirname, '../wwwroot'),
    },
    devtool: 'source-map',
    stats: {
        loggingDebug: ["sass-loader"],
    },
    module: {
        rules: [
            {
                test: /\.tsx?$/,
                use: 'ts-loader',
                exclude: /node_modules/,

            },
            {
                test: /\.scss$/,
                use: [
                    MiniCssExtractPlugin.loader,
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
                test: /\.(woff(2)?|ttf|eot|svg)(\?v=\d+\.\d+\.\d+)?$/,
                use: [
                    {
                        loader: 'file-loader',
                        options: {
                            name: '[name].[ext]',
                            outputPath: 'css/fonts/', // Output to a 'fonts' folder in your output directory
                        }
                    }
                ]
            }
        ],
    },
    plugins: [
        new HtmlWebpackPlugin({
            template: _resolve(__dirname, 'HtmlTemplates/index_template.html'),
            filename: 'index.html',
            scriptLoading: 'blocking', 
            
            inject: true,
            minify: false
        }),

        new MiniCssExtractPlugin({
            filename: 'css/[name].[contenthash].css',
            runtime: false,
            
        }),

        

    ],
    optimization: {
        splitChunks: {
          cacheGroups: {
            styles: {
              name: "styles",
              type: "css/mini-extract",
              chunks: "all",
                enforce: true,
            },
          },
        },
      },
    
    resolve: {
        extensions: ['.tsx', '.ts', '.js'],
    },

};