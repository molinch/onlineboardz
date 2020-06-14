import { appSchema, tableSchema, Model } from '@nozbe/watermelondb'
import { field } from '@nozbe/watermelondb/decorators'

export const gameSchema = appSchema({
  version: 1,
  tables: [
    tableSchema({
      name: 'gameTypes',
      columns: [
        { name: 'gameType', type: 'number', isIndexed: true },
        { name: 'minPlayers', type: 'number', },
        { name: 'maxPlayers', type: 'number' },
        { name: 'defaultDuration', type: 'number' },
      ]
    }),
  ]
})

export class GameType extends Model {
    static table = 'game_types'

    @field('gameType') gameType
    @field('minPlayers') minPlayers
    @field('maxPlayers') minPlayers
    @field('defaultDuration') minPlayers
}