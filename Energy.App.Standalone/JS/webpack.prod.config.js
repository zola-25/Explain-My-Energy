/*eslint no-undef: "error"*/
/*eslint-env node*/

const path = require("path");
const TerserPlugin = require("terser-webpack-plugin");
const LicenseWebpackPlugin = require('license-webpack-plugin').LicenseWebpackPlugin;


module.exports = {
    entry: './index.ts',
    mode: 'production',
    output: {
        filename: 'bundle.min.js',
        path: path.resolve(__dirname, '../wwwroot/js'),
        clean: true,
    },
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
            renderBanner: (filename, modules) => {
                console.log(modules);
                return '/*! licenses are at ' + filename + '*/';
            }
        })
    ],
    optimization: {
        minimize: true,
        minimizer: [new TerserPlugin({
            extractComments: false,
            terserOptions: {
                format: {
                    // Tell terser to remove all comments except for the banner added via LicenseWebpackPlugin.
                    // This can be customized further to allow other types of comments to show up in the final js file as well.
                    // See the terser documentation for format.comments options for more details.
                    comments: (astNode, comment) => comment.value.startsWith('! licenses are at ')
                        
                }

            },
            
        }
        )]
    },
    resolve: {
        extensions: ['.tsx', '.ts', '.js'],
    }
};
