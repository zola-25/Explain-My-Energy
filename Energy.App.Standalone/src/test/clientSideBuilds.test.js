import fs from 'fs';
import { describe, it, after, before } from 'mocha';
import 'chai/register-should.js';
import { load } from 'cheerio';
import { execSync } from 'child_process';
import { resolve as _resolve } from 'path';
import { clearDirectory } from '../clearDirectory.js';
import glob from 'glob';

const scriptDirectory = 'src/test';

const outputDirectory = _resolve(scriptDirectory, '../../wwwroot');

describe('buildClientSide', function () {

  const indexHtmlPath = _resolve(outputDirectory, 'index.html');
  const ClientsHtmlPath = _resolve(outputDirectory, 'Clients.html');

  const environments = ['development', 'staging', 'production'];

  environments.forEach((environment) => {

    let indexHtmlContents;
    let clientsHtmlContents;

    let $index;
    let $clients;

    before('build client side', function () {

      clearDirectory(outputDirectory, 'temp');
      this.timeout(1000 * 60 * 2);

      execSync(`node ./src/build.js --${environment}`, { stdio: 'inherit', env: process.env });

      indexHtmlContents = fs.readFileSync(indexHtmlPath, "utf8");
      clientsHtmlContents = fs.readFileSync(ClientsHtmlPath, "utf8");

      $index = load(indexHtmlContents);
      $clients = load(clientsHtmlContents);

    });

    after('cleanup client side', function () {

      clearDirectory(outputDirectory, 'temp');

      indexHtmlContents = null;
      clientsHtmlContents = null;

      $index = null;
      $clients = null;

    });

    it('should have css files', function () {

      const cssDir = _resolve(outputDirectory, 'css');
      const cssFilePrefixesToCheck = ['app', 'app-fontawesome', 'attribs'];


      glob.glob('*.css',{ cwd: cssDir}, function (err, files) {

        if (err) {
          throw err;
        }

        files.length.should.be.greaterThan(0);
        files.forEach((file) => {
          const parts = file.split('.');
          const filePrefix = parts[0];
          const fileSuffix = parts[parts.length - 1]
          cssFilePrefixesToCheck.should.include(filePrefix);

          if (fileSuffix === 'map') {

            return;
          }

          if (filePrefix === 'app') {
            $index(`link[href="css/${file}"]`).toArray().length.should.equal(1);
          } else if (filePrefix === 'app-fontawesome') {
            $index(`link[href="css/${file}"]`).toArray().length.should.equal(1);
          } else if (filePrefix === 'attribs') {
            $clients(`link[href="css/${file}"]`).toArray().length.should.equal(1);
          }

        });


      });
    });

    it('should have one js file', function () {

      const jsDir = _resolve(outputDirectory, 'js');

      glob.glob('*.js', {cwd: jsDir}, function (err, files) {

        if (err) {
          throw err;
        }

        files.length.should.equal(1);
        files.forEach((file) => {

          $index(`script[src="js/${file}"]`).toArray().length.should.equal(1);
        });
      });

    });

    it('should have noindex for robots if not production', function () {

      if (environment === 'production') {
        $index('meta[name="robots"]').attr('content').should.equal('index, follow');
      } else {
        $index('meta[name="robots"]').attr('content').should.equal('noindex, nofollow');
      }
    });

    it('should have some fonts in fonts/ directory', function () {

      const fontsDir = _resolve(outputDirectory, 'fonts');

      glob.glob(['*.woff2', '*.woff'], {cwd: fontsDir}, function (err, files) {

        if (err) {
          throw err;
        }

        files.all((file) => { file.startsWith('OpenSans-') }).should.be.true;

        files.length.should.be.equal(4);

      });
    });

    it('should have an image in images/ directory', function () {

      const imagesDir = _resolve(outputDirectory, 'images');

      glob.glob(['*.png'], {cwd: imagesDir}, function (err, files) {

        if (err) {
          throw err;
        }

        files.length.should.be.equal(1);
        $index(`meta [property="og:image"]`).attr('content').should.endWith('DecemberWeather.png');
      });
    });

    it('should have an ico favicon, an apple-touch-icon, and a manifest.json', function () {

      const faviconsDir = _resolve(outputDirectory);

      glob.glob(['*.ico', '*.png'], {cwd: faviconsDir}, function (err, files) {

        if(err)
        {
          throw err;
        }

        files.one((file) => { file === 'favicon.ico' }).should.be.true;
        files.one((file) => { file === 'apple-touch-icon.png' }).should.be.true;

        $index(`link[rel="icon"][href="/favicon.ico"]`).toArray().length.should.equal(1);
        $index(`link[rel="apple-touch-icon"][href="/apple-touch-icon.png"]`).toArray().length.should.equal(1);


      });

      outputDirectory.should.have.files(['manifest.json']);
      $index(`link[rel="manifest"][href="/manifest.json"]`).toArray().length.should.equal(1);

    });

  });

});

