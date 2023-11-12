
export * from './Files/types'
export * from './Files/chartCreation'

import { Charts } from './Files/chartCreation'

import FluxorPersistIndexedDb from './FluxorPersistIndexedDb'

console.log("Loading Charts...")
Charts.Load();

console.log("Loading FluxorPersistIndexedDb...")
FluxorPersistIndexedDb.Load();


