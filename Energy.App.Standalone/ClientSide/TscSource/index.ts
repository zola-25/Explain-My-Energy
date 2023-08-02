import './assets-path'
import 'tslib'


export * from './types'
export * from './chartCreation'
export * from './energyOnlyChart'
export * from './weatherIconChart'

import { Charts } from './chartCreation'

console.log("Initialize Charts...")

Charts.Initialize();