import fs, { rmSync } from 'fs';
import { describe, it, after, before } from 'mocha';
import 'chai/register-should.js';
import { load } from 'cheerio';
import { execSync } from 'child_process';
import { resolve as _resolve } from 'path';


const scriptDirectory = 'src/test';

const creditsDocPath = _resolve(scriptDirectory, '../views/Credits.html');
const environments = ['development', 'staging', 'production'];

environments.forEach((environment) => {

  describe('buildCreditsDoc', function () {


    let creditsDocHtml;
    before(`clear and build Credits.html for ${environment}`, function () {
      this.timeout(1000 * 60 * 1);

      if (fs.existsSync(creditsDocPath)) {
        rmSync(creditsDocPath);
      }

      process.env.APP_ENV = environment;

      execSync('node ./src/attributionAndLicensing/GenerateCreditsDoc.js', { stdio: 'inherit', env: process.env });

      creditsDocHtml = fs.readFileSync(creditsDocPath, "utf8");
    });

    it(`${environment} should have Explain My Energy license in Credits.html`, function () {

      const $ = load(creditsDocHtml);

      $('.eme--license-attribution-header--root .eme--license-type').text().trim().should.equal('Apache-2.0');

      $('.eme--license-attribution-header--root .eme--license-text pre code').text().length.should.be.above(100);

    });

    it(`${environment} should have some nuget and package credits`, function () {

      const $ = load(creditsDocHtml);

      $('.eme--thirdparty--nuget--package--container').toArray().length.should.be.greaterThan(2, "number of nuget packages should be at least 2");

      $('.eme--thirdparty--npm--package--container').toArray().length.should.be.greaterThan(1, "number of packages should be at least 1");

    });

    after(function () {
      creditsDocHtml = null;

      if (fs.existsSync(creditsDocPath)) {
        fs.rmSync(creditsDocPath);
      }

    });
  });
});
