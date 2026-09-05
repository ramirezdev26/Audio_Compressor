import { validateAudioHeader } from './audioValidation'

export interface AudioValidationMessage {
  fileBuffer: ArrayBuffer
  fileSize: number
}

self.onmessage = (event: MessageEvent<AudioValidationMessage>) => {
  const { fileBuffer, fileSize } = event.data
  const bytes = new Uint8Array(fileBuffer)
  const result = validateAudioHeader(bytes, fileSize)
  self.postMessage(result)
}
