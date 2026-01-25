export class FormHandler {
    cleanUpFormInputs(...formInputs: string[]): string[] {
        return formInputs.map((formInput) => {
            return formInput.trim()
        })
    }
}
