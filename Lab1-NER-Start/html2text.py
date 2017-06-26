import urllib
from bs4 import BeautifulSoup


def main():
    url = "http://www.bbc.com/news/entertainment-arts-40387824"
    html = urllib.urlopen(url).read()
    soup = BeautifulSoup(html, "lxml")

    # Remove all script and style elements
    for script in soup(["script", "style"]):
        script.extract()  # rip it out

    # Get text
    text = soup.get_text()

    # Break into lines and remove leading and trailing space on each
    lines = (line.strip() for line in text.splitlines())

    # Break multi-headlines into a line each
    chunks = (phrase.strip() for line in lines for phrase in line.split("  "))

    # Drop blank lines
    text = '\n'.join(chunk for chunk in chunks if chunk)

    # One word per line
    words = '\n'.join(word for word in text.split())

    print(words.encode('utf-8'))

if __name__ == '__main__':
    main()
