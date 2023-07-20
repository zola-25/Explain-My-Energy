const path = require("path");
const TerserPlugin = require("terser-webpack-plugin");


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
    optimization: {
        minimize: true,
        minimizer: [new TerserPlugin({
        })]
    },
    resolve: {
        extensions: ['.tsx', '.ts', '.js'],
    }
};
