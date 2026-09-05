const MAX_FILE_SIZE_BYTES = 50 * 1024 * 1024 // 50MB

export interface AudioValidationResult {
  valid: boolean
  reason?: string
}

function matchesAsciiHeader(bytes: Uint8Array, offset: number, ascii: string): boolean {
  for (let i = 0; i < ascii.length; i++) {
    if (bytes[offset + i] !== ascii.charCodeAt(i)) {
      return false
    }
  }
  return true
}

function isMp3(bytes: Uint8Array): boolean {
  if (matchesAsciiHeader(bytes, 0, 'ID3')) {
    return true
  }
  return bytes[0] === 0xff && (bytes[1] === 0xfb || bytes[1] === 0xfa)
}

function isWav(bytes: Uint8Array): boolean {
  return matchesAsciiHeader(bytes, 0, 'RIFF') && matchesAsciiHeader(bytes, 8, 'WAVE')
}

export function validateAudioHeader(bytes: Uint8Array, size: number): AudioValidationResult {
  if (size > MAX_FILE_SIZE_BYTES) {
    return { valid: false, reason: 'El archivo supera el tamaño máximo de 50MB' }
  }

  if (isMp3(bytes) || isWav(bytes)) {
    return { valid: true }
  }

  return { valid: false, reason: 'Formato de audio no soportado' }
}
