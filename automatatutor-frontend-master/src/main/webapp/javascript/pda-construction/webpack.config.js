const path = require('path');

module.exports = {
    optimization: {
        minimize: false
    },
    module: {
        rules: [
            {
                test: /\.css$/,
                use: ['style-loader', 'css-loader']
            }
        ]
    },
    mode: 'development',
    entry: {
        pda: './pda/pda.js',
        tm: './tm/tm.js'
    },
    output: {
        filename: '[name]-bundle.js',
        path: path.resolve(__dirname, 'out'),
        libraryTarget: 'var',
        library: '[name]Creator'
    }
};