import fs from 'fs';
import { describe, it, after, before } from 'mocha';
import 'chai/register-should.js';
import { load } from 'cheerio';
import { execSync } from 'child_process';
import { resolve as _resolve, extname } from 'path';
import { clearDirectory } from '../clearDirectory.js';
import { globSync } from 'glob';

const scriptDirectory = 'src/test';

const outputDirectory = _resolve(scriptDirectory, '../../wwwroot');

const environments = process.env.npm_config_environments ? process.env.npm_config_environments.split(",") : ['development', 'staging', 'production'];

environments.forEach((environment) => {

  describe('buildClientSide', function () {

    const indexHtmlPath = _resolve(outputDirectory, 'index.html');
    const CreditsHtmlPath = _resolve(outputDirectory, 'Credits.html');



    let indexHtmlContents;
    let creditsHtmlContents;

    let $index;
    let $credits;

    before(`${environment} build client side`, function () {

      if (fs.existsSync(outputDirectory)) {
        clearDirectory(outputDirectory, 'temp');
      }
      this.timeout(1000 * 60 * 2);

      execSync(`node ./src/build.js --${environment}`, { stdio: 'inherit' });

      indexHtmlContents = fs.readFileSync(indexHtmlPath, "utf8");
      creditsHtmlContents = fs.readFileSync(CreditsHtmlPath, "utf8");

      $index = load(indexHtmlContents);
      $credits = load(creditsHtmlContents);

    });

    after(`${environment} cleanup client side`, function () {

      if (fs.existsSync(outputDirectory)) {
        clearDirectory(outputDirectory, 'temp');
      }

      indexHtmlContents = null;
      creditsHtmlContents = null;

      $index = null;
      $credits = null;

    });

    it(`${environment} should have css files`, function () {

      const cssDir = _resolve(outputDirectory, 'css');
      const cssFilePrefixesToCheck = ['app', 'app-fontawesome', 'attribs'];


      const files = globSync('*.css', { cwd: cssDir })
      files.length.should.be.equal(3);

      files.forEach((file) => {
        const parts = file.split('.');
        const filePrefix = parts[0];
        const fileSuffix = parts[parts.length - 1]
        cssFilePrefixesToCheck.should.include(filePrefix);

        fileSuffix.should.equal('css');

        if (filePrefix === 'app') {
          $index(`link[href="/css/${file}"]`).toArray().length.should.equal(1);
        } else if (filePrefix === 'app-fontawesome') {
          $index(`link[href="/css/${file}"]`).toArray().length.should.equal(1);
        } else if (filePrefix === 'attribs') {
          $credits(`link[href="/css/${file}"]`).toArray().length.should.equal(1);
        }

      });

    });

    it(`${environment} should have one js file`, function () {

      const jsDir = _resolve(outputDirectory, 'js');

      const files = globSync('*.js', { cwd: jsDir })


      files.length.should.equal(1);
      files.forEach((file) => {

        $index(`script[src="/js/${file}"]`).toArray().length.should.equal(1);
      });

    });

    it(`${environment} should have noindex for robots for now, regardless of environment`, function () {

      if (environment === 'production') {
        $index('meta[name="robots"]').attr('content').should.equal('noindex, nofollow');
      } else {
        $index('meta[name="robots"]').attr('content').should.equal('noindex, nofollow');
      }
    });

    it(`${environment} should have some fonts in fonts/ directory`, function () {

      const fontsDir = _resolve(outputDirectory, 'fonts');

      const files = globSync(['*.woff2', '*.woff'], { cwd: fontsDir });

      files.map((file) => { return file.startsWith('OpenSans-') }).length.should.equal(4);

      files.length.should.be.equal(4);


    });

    it(`${environment} should have an image in images/ directory`, function () {

      const imagesDir = _resolve(outputDirectory, 'images');

      const files = globSync(['*.png'], { cwd: imagesDir })

      files.length.should.be.equal(1);
      $index(`meta[property="og:image"]`).attr('content').should.match(/DecemberWeather\.png/);

    });

    it(`${environment} should have an ico favicon, an apple-touch-icon, and a manifest.json`, function () {

      const faviconsDir = _resolve(outputDirectory);

      const files = globSync(['*.ico', '*.png'], { cwd: faviconsDir })

      files.filter((file) => { return extname(file) === '.png' }).length.should.equal(5);

      files.filter((file) => { return file === 'favicon.ico' }).length.should.equal(1);
      files.filter((file) => { return file === 'apple-touch-icon.png' }).length.should.equal(1);

      $index(`link[rel="icon"][href="/favicon.ico"]`).toArray().length.should.equal(1);
      $index(`link[rel="apple-touch-icon"][href="/apple-touch-icon.png"]`).toArray().length.should.equal(1);

      fs.readdirSync(outputDirectory).should.include('manifest.json');
      $index(`link[rel="manifest"][href="manifest.json"]`).toArray().length.should.equal(1);

    });

  });

});

