/* 

import gulp from 'gulp';
import del from 'del';
import sourcemaps from 'gulp-sourcemaps'
import babel from 'gulp-babel'
import rename from 'gulp-rename'
import buffer from 'vinyl-buffer'
import terser from 'gulp-terser'
import tsnode from 'ts-node'
import  tsc  from 'typescript';


tsc.bundlerModuleNameResolver = function (moduleName, containingFile, compilerOptions, host) {

let tsNodeService = tsnode.create({ 
    project: './tsconfig.json',    
  });

  tsNodeService.ts.displayPartsToString.createSourceFile({
    fileName: 'index.ts',
  });    

  tsNodeService.compile(() )


gulp.task('scripts', function() {
    return gulp.src('Files/*.ts')
        .pipe(sourcemaps.init()) 
        .pipe(tsProject())
        .pipe(babel()).on("error",
            error => {
                console.log(error.toString());
                this.emit("end");
            })
        .pipe(gulp.dest('../wwwroot/js/'));
});

const cleanJs = async () =>
{
    return del.deleteAsync(["../wwwroot/js/"]);
}

const cleanCss = async () => {
    return del.deleteAsync(["../wwwroot/css/*.css"]);
};


exports.default = defaultTask */