const path = require("path");
const HtmlWebpackPlugin = require("html-webpack-plugin");
const { CleanWebpackPlugin } = require("clean-webpack-plugin");
const MiniCssExtractPlugin = require("mini-css-extract-plugin");

module.exports = {
    entry: "./src/index.ts",
    // overrides the default value of dist and emit the result to wwwroot directory
    output: {
        path: path.resolve(__direname, "wwwroot"),
        filename="[name].[chunkhash].js",
        publicPath: "/"
    },
    //includes .js to import the SignalR client JavaScript.
    resolve: {
        extensions:[".js", ".ts"]
    },
    module: {
        rules: [
            {
                test: /\.ts$/,
                use:"ts-loader"
            },
            {
                test: /\.css$/,
                use: [MiniCssExtractPlugin.loader, "css-loader"]
            }
        ]
    },
    plugins: [
        new CleanWebpackPlugin(),
        new HtmlWebpackPlugin({
            template: "./src/index.html"
        }),
        new MiniCssExtractPlugin({
            filename: "css/[name].[chunkhash].css"
        })
    ]

};