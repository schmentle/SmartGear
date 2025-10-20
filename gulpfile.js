const { src, dest, series, parallel, watch } = require('gulp');
const sass = require('gulp-sass')(require('sass'));
const cleanCSS = require('gulp-clean-css');
const sourcemaps = require('gulp-sourcemaps');
const concat = require('gulp-concat');
const uglify = require('gulp-uglify');
const rename = require('gulp-rename');
const del = require('del');

const paths = {
    scss: 'Assets/scss/site.scss',
    js: [
        'node_modules/jquery/dist/jquery.js',
        'node_modules/@popperjs/core/dist/umd/popper.js',
        'node_modules/bootstrap/dist/js/bootstrap.js',
        'Assets/js/site.js'
    ],
    outCss: 'wwwroot/css',
    outJs: 'wwwroot/js'
};

async function clean() {
    const { deleteAsync } = await import('del');
    await deleteAsync([paths.outCss, paths.outJs]);
}

function styles() {
    return src(paths.scss)
        .pipe(sourcemaps.init())
        .pipe(sass().on('error', sass.logError))
        .pipe(dest(paths.outCss))
        .pipe(cleanCSS())
        .pipe(rename({ suffix: '.min' }))
        .pipe(sourcemaps.write('.'))
        .pipe(dest(paths.outCss));
}

function scripts() {
    return src(paths.js, { allowEmpty: true })
        .pipe(sourcemaps.init())
        .pipe(concat('bundle.js'))
        .pipe(dest(paths.outJs))
        .pipe(uglify())
        .pipe(rename({ suffix: '.min' }))
        .pipe(sourcemaps.write('.'))
        .pipe(dest(paths.outJs));
}

function watcher() {
    watch('Assets/scss/**/*.scss', styles);
    watch('Assets/js/**/*.js', scripts);
}

exports.clean = clean;
exports.build = series(clean, parallel(styles, scripts));
exports.watch = series(exports.build, watcher);