import { describe, expect, it } from 'vitest'
import { validateAudioHeader } from './audioValidation'

function bytesFrom(values: number[]): Uint8Array {
  return new Uint8Array(values)
}

describe('validateAudioHeader', () => {
  it('acepta un archivo mp3 válido (header ID3)', () => {
    const bytes = bytesFrom([0x49, 0x44, 0x33, 0x03, 0x00, 0x00, 0x00])
    const result = validateAudioHeader(bytes, 1024)

    expect(result.valid).toBe(true)
    expect(result.reason).toBeUndefined()
  })

  it('acepta un archivo wav válido (header RIFF/WAVE)', () => {
    const bytes = bytesFrom([
      0x52, 0x49, 0x46, 0x46, 0x00, 0x00, 0x00, 0x00, 0x57, 0x41, 0x56, 0x45,
    ])
    const result = validateAudioHeader(bytes, 2048)

    expect(result.valid).toBe(true)
  })

  it('rechaza un archivo con header incorrecto', () => {
    const bytes = bytesFrom([0x00, 0x00, 0x00, 0x00])
    const result = validateAudioHeader(bytes, 1024)

    expect(result.valid).toBe(false)
    expect(result.reason).toBe('Formato de audio no soportado')
  })

  it('rechaza un archivo que excede el tamaño máximo', () => {
    const bytes = bytesFrom([0x49, 0x44, 0x33])
    const oversizedFile = 50 * 1024 * 1024 + 1

    const result = validateAudioHeader(bytes, oversizedFile)

    expect(result.valid).toBe(false)
    expect(result.reason).toBe('El archivo supera el tamaño máximo de 50MB')
  })
})
