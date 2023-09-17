/* eslint-disable no-undef */
/* eslint-disable @typescript-eslint/no-var-requires */

import { resolve as _resolve, dirname, posix } from "path";
import { fileURLToPath } from 'url';
import * as sass from 'sass'
import HtmlBundlerPlugin from 'html-bundler-webpack-plugin'
import CopyPlugin from 'copy-webpack-plugin'
import TerserPlugin from "terser-webpack-plugin";
import appInfoConfig from "./appInfoConfig.js";



export default function (appEnv, appDemo) {

    const envAppInfoConfig = appInfoConfig(appEnv);
    const __dirname = dirname(fileURLToPath(import.meta.url));

    console.log("__dirname resolves: " + __dirname);

    const copyPatterns = [{
        from: posix.join(_resolve(__dirname, 'favicons/').replace(/\\/g, '/'), '*.png'),
        to: "[name][ext]",
    },
    {
        from: posix.join(_resolve(__dirname, 'images/').replace(/\\/g, '/')),
        to: "images/",
    },
    {
        from: posix.join(_resolve(__dirname, 'data/staticwebapp.config.prod.json').replace(/\\/g, '/')),
        to: "staticwebapp.config.json"
    },
    {
        from: posix.join(_resolve(__dirname, 'data/manifest.webmanifest').replace(/\\/g, '/')),
        to: "[name][ext]"
    },
    {
        from: posix.join(_resolve(__dirname, 'data/robots.disallow.txt').replace(/\\/g, '/')),
        to: "robots.txt",
    },
    ];
    if (appDemo) {
        copyPatterns.push(
            {
                from: posix.join(_resolve(__dirname, 'data/appsettings.json').replace(/\\/g, '/')),
                to: "appsettings.json"
            },
            {
                from: posix.join(_resolve(__dirname, 'data/demo/').replace(/\\/g, '/')),
                to: "demo/"
            })
    }

    const productionConfig = {
        resolve: {
            extensions: ['.tsx', '.ts', '.js'],
        },
        plugins: [
            new HtmlBundlerPlugin({
                // define a relative or absolute path to entry templates
                outputPath: _resolve(__dirname, '../wwwroot'),
                entry: {
                    index: {
                        import: _resolve(__dirname, 'views/index_template.ejs'),
                        data: envAppInfoConfig.headerData,
                    },
                    Credits: {
                        import: _resolve(__dirname, 'views/Credits.html'),

                    }
                },
                preprocessor: 'ejs',
                preprocessorOptions: {
                    async: false,
                    root: _resolve(__dirname, 'views'),
                    views: [_resolve(__dirname, 'views')]
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
                            filter: ({ value }) => {
                                if (value.startsWith('_content') ||
                                    value.startsWith('_framework') ||
                                    value.startsWith('Energy.App.Standalone') ||
                                    value.startsWith('manifest.webmanifest')) {
                                    return false;
                                }
                                return true;

                            }
                        },
                        {
                            tag: 'script',
                            attributes: ['src'],
                            filter: ({ value }) => {
                                if (value.startsWith('_content') ||
                                    value.startsWith('_framework') ||
                                    value.startsWith('Energy.App.Standalone')) {
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
            new CopyPlugin({
                patterns: copyPatterns
            })

        ],
        mode: 'production',
        devtool: 'hidden-source-map',
        output: {
            path: _resolve(__dirname, '../wwwroot'),
            publicPath: '/',
            clean: {
                keep: /temp/
            }
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
                    }
                },
                {
                    test: /\.(woff2|woff|ttf)$/,
                    type: 'asset/resource',
                    include: _resolve(__dirname, 'scss/fontawesome/webfonts/'),
                    generator: {
                        filename: 'webfonts/[name].[contenthash:8][ext]',
                    }
                },
                {
                    test: /\.(ico|png|svg)$/,
                    type: 'asset/resource',
                    include: _resolve(__dirname, 'favicons/'),

                    generator: {
                        filename: '[name][ext]',

                    }
                }
            ],
        },
        optimization: {
            minimize: true,
            minimizer: [new TerserPlugin({
                extractComments: "some",
                terserOptions: {
                    sourceMap: true,
                },
            })]
        }
    };

    return productionConfig;
}