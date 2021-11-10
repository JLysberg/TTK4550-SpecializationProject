import ETDataInterface as et

interface = et.ETDataInterface()
df = interface._timeSeries[0]

print(df)
