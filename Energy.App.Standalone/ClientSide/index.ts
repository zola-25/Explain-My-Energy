
export * from './Files/types'
export * from './Files/chartCreation'
export * from './Files/energyOnlyChart'
export * from './Files/weatherIconChart'

import { Charts } from './Files/chartCreation'

console.log("Loading Charts...")

Charts.Load();