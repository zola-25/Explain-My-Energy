/* eslint-disable no-undef */

const path = require('path');
const webpack = require('webpack');

const ASSETS_PATH = process.env.ASSETS_PATH || '/assets/';


let commonConfig = {
    context: path.resolve(__dirname),
    output: {
        environment: {
            arrowFunction: false,
        },

        publicPath: process.env.ASSETS_PATH,
    },
    plugins: [
        new webpack.DefinePlugin({
            'process.env.ASSETS_PATH': JSON.stringify(ASSETS_PATH),
        })
    ],

}



