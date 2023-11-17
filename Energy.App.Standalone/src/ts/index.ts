import { ChartFunctions } from './Files/chartCreation'
import { LocalStorage } from './Files/localStorage'

ChartFunctions.Load();

console.log("Loaded Charts...Done");

LocalStorage.load();

console.log("Local Storage Helper...Done");
