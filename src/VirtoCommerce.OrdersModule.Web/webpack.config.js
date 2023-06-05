const moduleId = "VirtoCommerce.Orders"

const glob = require("glob");
const path = require("path");
const webpack = require("webpack");
const MiniCssExtractPlugin = require("mini-css-extract-plugin");
const { CleanWebpackPlugin } = require("clean-webpack-plugin");

const rootPath = path.resolve(__dirname, "dist");

function getEntrypoints() {
    const options = { nosort: true, dotRelative: true, posix: true };
    const result = [
        ...glob.sync("./Scripts/**/*.js", options),
        ...glob.sync("./Content/**/*.css", options)
    ];

    return result;
}

module.exports = (env, argv) => {
    const isProduction = argv.mode === "production";

    return {
        entry: getEntrypoints(),
        devtool: false,
        output: {
            path: rootPath,
            filename: "app.js"
        },
        module: {
            rules: [
                {
                    test: /\.css$/,
                    use: [MiniCssExtractPlugin.loader, "css-loader"]
                }
            ]
        },
        plugins: [
            new CleanWebpackPlugin(),
            isProduction ?
                new webpack.SourceMapDevToolPlugin({
                    namespace: moduleId,
                    filename: "[file].map[query]"
                }) :
                new webpack.SourceMapDevToolPlugin({
                    namespace: moduleId
                }),
            new MiniCssExtractPlugin({
                filename: "style.css"
            })
        ]
    };
};

