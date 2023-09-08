export async function copyToClipboard(inputText) {

    const result = await navigator.clipboard.writeText(inputText).then(function () {
            return true; 
        })
        .catch(function (error) {
            console.error(error);
            return false;
        });

    return result
}