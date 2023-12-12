import { ChartFunctions } from './Files/chartCreation'
import { LocalStorage } from './Files/localStorage'
import { StorageHelper } from './Files/storageHelper'

ChartFunctions.Load();

console.log("Loaded Charts...Done");

LocalStorage.load();
StorageHelper.Load();

console.log("Local Storage Helper...Done");
