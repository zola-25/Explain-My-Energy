import assert from 'assert'
import { clearDirectory } from '../clearDirectory.js';
import fs from 'fs';
import { describe, it, after } from 'mocha';

const scriptDirectory = 'src/test';
const testDirectory = `${scriptDirectory}/testDir`;

describe('clearDirectory', () => {

  it('should delete all files in the directory', () => {
    // Set up a test directory with some files
    fs.mkdirSync(testDirectory);
    fs.writeFileSync(`${testDirectory}/file1.txt`, 'test content');
    fs.writeFileSync(`${testDirectory}/file2.txt`, 'more test content');

    // Call the clearDirectory function
    clearDirectory(testDirectory);

    // Check that the directory is now empty
    const files = fs.readdirSync(testDirectory);
    assert.deepStrictEqual(files, []);
  });

  after(() => {
    fs.rmSync(testDirectory, { recursive: true });
  });
});

describe('clearDirectory excluding a folder', () => {
  it('should delete all files in the directory', () => {
    // Set up a test directory with some files
    fs.mkdirSync(testDirectory);
    fs.writeFileSync(`${testDirectory}/file1.txt`, 'test content');
    fs.writeFileSync(`${testDirectory}/file2.txt`, 'more test content');

    fs.mkdirSync(`${testDirectory}/exclude`);
    fs.writeFileSync(`${testDirectory}/exclude/file1.txt`, 'test content');
    fs.writeFileSync(`${testDirectory}/exclude/file2.txt`, 'more test content');

    // Call the clearDirectory function
    clearDirectory(testDirectory, 'exclude');

    // Check that the directory is now empty
    const files = fs.readdirSync(testDirectory);

    assert.deepStrictEqual(files, ['exclude']);
  });

  after(() => {
    fs.rmSync(testDirectory, { recursive: true });
  });
});